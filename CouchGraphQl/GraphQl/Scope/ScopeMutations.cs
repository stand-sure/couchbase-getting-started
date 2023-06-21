namespace CouchGraphQl.GraphQl.Scope;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Couchbase.Management.Collections;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class ScopeMutations
{
    public async Task<bool> CreateScopeAsync(
        string scopeName,
        string bucketName,
        [Service] IBucketProvider bucketProvider,
        [Service] ILogger<ScopeMutations> logger,
        CancellationToken cancellationToken)
    {
        bool success;

        try
        {
            IBucket bucket = await bucketProvider.GetBucketAsync(bucketName).ConfigureAwait(false);

            await bucket.Collections.CreateScopeAsync(scopeName,
                options =>
                {
                    options.CancellationToken(cancellationToken);
                });

            success = true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Class}.{Method} {Message}", nameof(ScopeMutations), nameof(this.CreateScopeAsync), e.Message);
            success = false;
        }

        return success;
    }
}