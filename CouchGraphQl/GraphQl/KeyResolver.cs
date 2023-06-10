namespace CouchGraphQl.GraphQl;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;

using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[UsedImplicitly]
internal class KeyResolver
{
    private KeyResolver()
    {
    }

    public static async Task<string?> GetKey<T>(
        [Service] INamedBucketProvider namedBucketProvider,
        [Parent] T document)
        where T : IEntityWithId<int>
    {
        IBucket bucket = await namedBucketProvider.GetBucketAsync().ConfigureAwait(false);

        var context = new BucketContext(bucket);

        IQueryable<T> records = context.Query<T>();

        string? key = (from record in records
            where record.Id == document.Id
            select N1QlFunctions.Key(record)).SingleOrDefault();
        
        return key;
    }
}