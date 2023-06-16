namespace CouchGraphQl;

using System.Reflection;

using Couchbase.Extensions.Tracing.Otel.Tracing;

using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

internal static partial class ProgramConfiguration
{
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
        providerBuilder.AddCouchbaseInstrumentation();

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
}