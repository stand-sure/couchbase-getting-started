namespace CouchGraphQl.GraphQl.Collection;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Management.Collections;

using JetBrains.Annotations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class CollectionMutations
{
    [UsedImplicitly]
    public async Task<bool> CreateCollectionAsync(
        string bucketName,
        string scopeName,
        string collectionName,
        [Service] IBucketProvider bucketProvider,
        [Service] ILogger<CollectionMutations> logger)

    {
        bool success;

        try
        {
            IBucket bucket = await bucketProvider.GetBucketAsync(bucketName).ConfigureAwait(false);
            await bucket.Collections.CreateCollectionAsync(new CollectionSpec(scopeName, collectionName));
            success = true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Class}.{Method} {Error}", nameof(CollectionMutations), nameof(this.CreateCollectionAsync), e.Message);
            success = false;
        }
        
        return success;
    }
}