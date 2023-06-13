namespace CouchGraphQl.GraphQl;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;

using CouchGraphQl.Data.Documents;

using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution.Processing;

using JetBrains.Annotations;

using RequestDelegate = Microsoft.AspNetCore.Http.RequestDelegate;

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

        IEnumerable<string> keys = (from record in records
            where record.Id == document.Id && record.Type == document.Type
            select N1QlFunctions.Key(record)).ToList();

        return keys.SingleOrDefault();
    }
}