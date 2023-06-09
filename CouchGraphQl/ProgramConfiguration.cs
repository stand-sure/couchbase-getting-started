namespace CouchGraphQl;

using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;

using CouchGraphQl.GraphQl;

using Prometheus;

using Serilog;

internal static class ProgramConfiguration
{
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

    internal static void ServiceConfiguration(HostBuilderContext context, IServiceCollection services)
    {
        services.AddLogging();
        services.AddHealthChecks();

        services.AddCouchbase(options =>
        {
            context.Configuration.GetSection("couchbase").Bind(options);
            options.AddLinq();
        }).AddCouchbaseBucket<INamedBucketProvider>("travel-sample");

        services.AddGraphQLServer()
            .AddQueryType<Query>();
    }

    private static void HostConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("appsettings.json");
        configurationBuilder.AddUserSecrets<Program>();
    }

    private static void LoggerConfiguration(HostBuilderContext context, IServiceProvider provider, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(provider);
    }
}