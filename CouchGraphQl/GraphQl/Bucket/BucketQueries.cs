namespace CouchGraphQl.GraphQl.Bucket;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Management.Buckets;

using JetBrains.Annotations;

[PublicAPI]
[ExtendObjectType(OperationTypeNames.Query)]
public class BucketQueries
{
    [UsedImplicitly]
    public async Task<IEnumerable<BucketSettings>> GetBucketsAsync(
        [Service] IClusterProvider clusterProvider)
    {
        ICluster cluster = await clusterProvider.GetClusterAsync().ConfigureAwait(false);
        IBucketManager bucketManager = cluster.Buckets;

        Dictionary<string, BucketSettings> buckets = await bucketManager.GetAllBucketsAsync().ConfigureAwait(false);

        IEnumerable<BucketSettings> settings = buckets.Values.Select(bs => bs);

        return settings;
    }

    public async Task<bool> CheckBucketExistenceAsync(
        string bucketName,
        [Service] IClusterProvider clusterProvider)
    {
        ICluster cluster = await clusterProvider.GetClusterAsync().ConfigureAwait(false);
        IBucketManager bucketManager = cluster.Buckets;

        bool exists;

        try
        {
            await bucketManager.GetBucketAsync(bucketName).ConfigureAwait(false);
            exists = true;
        }
        catch
        {
            exists = false;
        }

        return exists;
    }
}