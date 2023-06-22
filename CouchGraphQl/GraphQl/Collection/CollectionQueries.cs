namespace CouchGraphQl.GraphQl.Collection;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Management.Collections;

using CouchGraphQl.GraphQl.Scope;

using JetBrains.Annotations;

[ExtendObjectType(OperationTypeNames.Query)]
public class CollectionQueries
{
    [UsedImplicitly]
    public async Task<IEnumerable<string>> GetCollectionsAsync(
        string bucketName,
        string scopeName,
        [Service] IBucketProvider bucketProvider,
        [Service] ILogger<ScopeQueries> logger,
        CancellationToken cancellationToken)
    {
        IBucket bucket = await bucketProvider.GetBucketAsync(bucketName).ConfigureAwait(false);

        ICouchbaseCollectionManager collectionManager = bucket.Collections;

        IEnumerable<ScopeSpec> scopes =
            await collectionManager.GetAllScopesAsync(options => { options.CancellationToken(cancellationToken); }).ConfigureAwait(false);

        ScopeSpec? scope = scopes.SingleOrDefault(spec => spec.Name == scopeName);

        IEnumerable<string>? collections = scope?.Collections.Select(spec => spec.Name);

        return collections ?? Array.Empty<string>();
    }
}