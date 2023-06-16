namespace CouchGraphQl;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Extensions.Tracing.Otel.Tracing;
using Couchbase.Linq;

using CouchGraphQl.Data;
using CouchGraphQl.Data.Documents;

internal static partial class ProgramConfiguration
{
    private static void AddCouchbaseAndDataObjects(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCouchbase(options => ConfigureClusterOptions(options, configuration, services))
            .AddCouchbaseBucket<INamedBucketProvider>("travel-sample");

        services.AddSingleton(BucketFactory);

        services.AddTransient<MyBucketContext>();
        services.AddScoped<AirlineAccessor>();
    }

    private static IBucket BucketFactory(IServiceProvider provider)
    {
        var namedBucketProvider = ActivatorUtilities.GetServiceOrCreateInstance<INamedBucketProvider>(provider);

        Task<IBucket> task = namedBucketProvider.GetBucketAsync().AsTask();

        return task.GetAwaiter().GetResult();
    }

    private static void ConfigureClusterOptions(ClusterOptions options, IConfiguration configuration, IServiceCollection services)
    {
        configuration.GetSection("couchbase").Bind(options);
        options.AddLinq();

        options.WithLogging(services.BuildServiceProvider().GetService<ILoggerFactory>());

        options.WithTracing(tracingOptions =>
        {
            tracingOptions.Enabled = true;
            tracingOptions.RequestTracer = new OpenTelemetryRequestTracer();
        });
    }
}