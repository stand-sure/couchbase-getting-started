namespace StartUsingConsole;

using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using StartUsingConsole.HostedServices;

internal static class ProgramConfiguration
{
    public static void ConfigureHostBuilder(this IHostBuilder builder)
    {
        builder.UseSerilog(LoggerConfiguration);

        builder.ConfigureHostConfiguration(HostConfiguration);

        builder.ConfigureServices(ServiceConfiguration);
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

    private static void ServiceConfiguration(HostBuilderContext context, IServiceCollection services)
    {
        services.AddLogging();

        services.AddCouchbase(options =>
        {
            context.Configuration.GetSection("couchbase").Bind(options);
            options.AddLinq();
        }).AddCouchbaseBucket<INamedBucketProvider>("travel-sample");

        services.AddHostedService<CouchbaseDemoService>();
        services.AddHostedService<TcpHealthProbeService>();

        services.AddHealthChecks();
    }
}