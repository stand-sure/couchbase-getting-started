namespace CouchGraphQl;

using System.Reflection;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;

using CouchGraphQl.Data;
using CouchGraphQl.GraphQl;

using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Options;
using HotChocolate.Execution.Pipeline.Complexity;
using HotChocolate.Types.Pagination;

using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Prometheus;

using Serilog;

internal static class ProgramConfiguration
{
    private const string MessageTemplate = $"{nameof(ProgramConfiguration)}: {{Message}}";

    internal static void ConfigureApplicationBuilder(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();
        app.UseHttpMetrics();
    }

    internal static void ConfigureEndpointRouteBuilder(this IEndpointRouteBuilder app)
    {
        app.MapMetrics("/metricsz");
        app.MapHealthChecks("/healthz");
        app.MapGraphQL();
    }

    internal static void ConfigureHostBuilder(this IHostBuilder builder)
    {
        builder.UseSerilog(LoggerConfiguration);

        builder.ConfigureHostConfiguration(HostConfiguration);
    }

    internal static void ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddLogging();
        services.AddHealthChecks();

        services.AddCouchbase(options => ConfigureClusterOptions(options, configuration))
            .AddCouchbaseBucket<INamedBucketProvider>("travel-sample");

        services.AddSingleton(BucketFactory);

        services.AddTransient<MyBucketContext>();
        services.AddScoped<AirlineAccessor>();

        services.AddGraphQLServer()
            .ModifyOptions(ConfigureSchemaOptions)
            .AddQueries()
            .AddTypes()
            .AddMutations()
            .AddGlobalObjectIdentification()
            .AddQueryFieldToMutationPayloads()
            .ModifyRequestOptions(ConfigureRequestExecutorOptions)
            .SetPagingOptions(GetPagingOptions())
            .AddSorting()
            .AddFiltering()
            .AddErrorFilter<DetailRemovingErrorFilter>()
            .AllowIntrospection(true);

        services.AddInstrumentation(environment, configuration);
    }

    private static void AddInstrumentation(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        string serviceName = environment.ApplicationName;
        var version = typeof(Program).GetTypeInfo().Assembly.GetName().Version?.ToString();
        ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName, version);

        services.AddOpenTelemetry().WithTracing(providerBuilder =>
        {
            providerBuilder.ConfigureTraceProvider(resourceBuilder, configuration, environment, serviceName);
        });
    }

    private static IRequestExecutorBuilder AddMutations(this IRequestExecutorBuilder builder)
    {
        return builder.AddMutationType(descriptor => descriptor.Name(OperationTypeNames.Mutation))
            .AddTypeExtension<AirlineMutations>();
    }

    private static IRequestExecutorBuilder AddQueries(this IRequestExecutorBuilder builder)
    {
        return builder.AddQueryType(descriptor => descriptor.Name(OperationTypeNames.Query))
            .AddTypeExtension<AirlineQueries>();
    }

    private static IRequestExecutorBuilder AddTypes(this IRequestExecutorBuilder builder)
    {
        return builder.AddType<AirlineType>();
    }

    private static IBucket BucketFactory(IServiceProvider provider)
    {
        var namedBucketProvider = ActivatorUtilities.GetServiceOrCreateInstance<INamedBucketProvider>(provider);

        Task<IBucket> task = namedBucketProvider.GetBucketAsync().AsTask();

        return task.GetAwaiter().GetResult();
    }

    private static int ComplexityCalculator(ComplexityContext context)
    {
        int cost = context.Complexity + context.ChildComplexity;

        Log.Debug(ProgramConfiguration.MessageTemplate, $"Cost: {context.Selection.Name} {cost}");

        return cost;
    }

    private static void ConfigureClusterOptions(ClusterOptions options, IConfiguration configuration)
    {
        configuration.GetSection("couchbase").Bind(options);
        options.AddLinq();
    }

    private static void ConfigureRequestExecutorOptions(IServiceProvider provider, RequestExecutorOptions options)
    {
        options.IncludeExceptionDetails = true;
        options.Complexity.Enable = true;
        options.Complexity.ApplyDefaults = true;

        options.Complexity.Calculation = ComplexityCalculator;
    }

    private static void ConfigureSchemaOptions(SchemaOptions options)
    {
        options.SortFieldsByName = true;
        options.EnableFlagEnums = true;
        options.RemoveUnreachableTypes = true;
    }

    private static void ConfigureTraceProvider(
        this TracerProviderBuilder providerBuilder,
        ResourceBuilder resourceBuilder,
        IConfiguration configuration,
        IHostEnvironment environment,
        string serviceName)
    {
        providerBuilder.AddSource(serviceName).SetResourceBuilder(resourceBuilder);
        providerBuilder.AddHttpClientInstrumentation();
        providerBuilder.AddAspNetCoreInstrumentation();
        providerBuilder.AddHotChocolateInstrumentation();

        providerBuilder.AddJaegerExporter(options =>
        {
            string? endpointUriAddress = configuration["JaegerExporter:EndpointUri"];

            bool goodUri = Uri.TryCreate(endpointUriAddress, UriKind.Absolute, out Uri? endPointUri);

            if (goodUri is false)
            {
                return;
            }

            options.Endpoint = endPointUri;
            options.Protocol = JaegerExportProtocol.HttpBinaryThrift;
        });

        if (environment.IsDevelopment())
        {
            providerBuilder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Console);
        }
    }

    private static PagingOptions GetPagingOptions()
    {
        return new PagingOptions() { MaxPageSize = 100, IncludeTotalCount = true };
    }

    private static void HostConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("appsettings.json");
        configurationBuilder.AddJsonFile("appsettings.secret.json", optional: true); // this will be mounted in k8s and should not exist locally
        configurationBuilder.AddUserSecrets<Program>(); // put the credentials for Vault in secrets

        IConfiguration c = configurationBuilder.Build();

        IConfigurationSection configurationSection = c.GetSection("Vault");

        configurationBuilder.AddVault(options => { configurationSection.Bind(options); });
    }

    private static void LoggerConfiguration(HostBuilderContext context, IServiceProvider provider, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(provider);
    }
}