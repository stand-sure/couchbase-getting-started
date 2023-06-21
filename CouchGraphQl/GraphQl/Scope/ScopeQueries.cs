namespace CouchGraphQl.GraphQl.Scope;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Management.Collections;

[ExtendObjectType(OperationTypeNames.Query)]
public class ScopeQueries
{
    public async Task<IEnumerable<Scope>> GetScopesInBucketAsync(
        string bucketName,
        [Service] IBucketProvider bucketProvider,
        [Service] ILogger<ScopeQueries> logger,
        CancellationToken cancellationToken)
    {
        IBucket bucket = await bucketProvider.GetBucketAsync(bucketName).ConfigureAwait(false);

        IEnumerable<ScopeSpec> scopeSpecs = await bucket.Collections.GetAllScopesAsync().ConfigureAwait(false);

        IEnumerable<Scope> scopes = scopeSpecs.Select(spec => new Scope(spec.Name, spec.Collections.Select(x => x.Name)));

        return scopes;
    }
}