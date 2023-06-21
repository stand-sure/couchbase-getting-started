namespace CouchGraphQl.GraphQl.Bucket;

using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Couchbase.Management.Buckets;

using CouchGraphQl.Data;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class BucketMutations
{
    public async Task<bool> CreateBucketAsync(
        string bucketName,
        [Service] MyBucketContext context,
        [Service] ILogger<BucketMutations> logger)
    {
        IBucketManager bucketManager = context.Bucket.Cluster.Buckets;

        try
        {
            await bucketManager.CreateBucketAsync(new BucketSettings
            {
                BucketType = BucketType.Couchbase,
                Name = bucketName,
                StorageBackend = StorageBackend.Couchstore,
                RamQuotaMB = 8964,
                EvictionPolicy = EvictionPolicyType.ValueOnly,
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Class}.{Method}: {Message}", nameof(BucketMutations), nameof(this.CreateBucketAsync), e.Message);
        }

        var c = ActivatorUtilities.GetServiceOrCreateInstance<IClusterProvider>(context.Bucket.Cluster.ClusterServices);
        bool success = await new BucketQueries().CheckBucketExistenceAsync(bucketName, c).ConfigureAwait(false);

        return success;
    }
}