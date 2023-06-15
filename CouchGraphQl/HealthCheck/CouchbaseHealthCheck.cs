namespace CouchGraphQl.HealthCheck;

using Couchbase;
using Couchbase.Diagnostics;
using Couchbase.Extensions.DependencyInjection;

using Microsoft.Extensions.Diagnostics.HealthChecks;

internal class CouchbaseHealthCheck : IHealthCheck
{
    private readonly INamedBucketProvider bucketProvider;

    public CouchbaseHealthCheck(INamedBucketProvider bucketProvider)
    {
        this.bucketProvider = bucketProvider ?? throw new ArgumentNullException(nameof(bucketProvider));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        IBucket bucket = await this.bucketProvider.GetBucketAsync();
        IPingReport pingReport = await PingAsync(bucket, cancellationToken).ConfigureAwait(false);

        bool healthy = pingReport.Services.All(pair => pair.Value.All(diagnostics => diagnostics.State == ServiceState.Ok));

        HealthStatus status = healthy ? HealthStatus.Healthy : HealthStatus.Unhealthy;
        
        var result = new HealthCheckResult(status);

        return result;
    }

    private static Task<IPingReport> PingAsync(IBucket bucket, CancellationToken cancellationToken) =>
        bucket.PingAsync(options =>
        {
            options.CancellationToken(cancellationToken);
            options.ServiceTypes(ServiceType.Query);
        });
}