namespace CouchGraphQl.HealthCheck;

internal static class CouchbaseHealthCheckExtensions
{
    public static IHealthChecksBuilder AddCouchbaseHealthCheck(this IHealthChecksBuilder builder)
    {
        builder.AddCheck<CouchbaseHealthCheck>(nameof(Couchbase), tags: new[] { nameof(Couchbase) });

        return builder;
    }
}