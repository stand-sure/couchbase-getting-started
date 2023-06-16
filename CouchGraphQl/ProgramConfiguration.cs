namespace CouchGraphQl;

using CouchGraphQl.HealthCheck;

using Prometheus;

using Serilog;

internal static partial class ProgramConfiguration
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

    internal static void ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddLogging();
        
        services.AddHealthChecks()
            .AddCouchbaseHealthCheck();
        
        services.AddCouchbaseAndDataObjects(configuration);
        services.AddGraphQl();
        services.AddInstrumentation(environment, configuration);
    }

    private static void HostConfiguration(IConfigurationBuilder configurationBuilder)
    {
        // this will be mounted in k8s and should not exist locally
        configurationBuilder.AddJsonFile("appsettings.secret.json", true);

        // put the credentials for Vault in secrets
        configurationBuilder.AddUserSecrets<Program>();

        configurationBuilder.AddVault();
    }
}