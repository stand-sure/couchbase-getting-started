namespace CouchGraphQl.GraphQl;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;

using CouchGraphQl.Data.Documents;

public class Query
{
    public async Task<IQueryable<Airline>> GetAirlines([Service] INamedBucketProvider namedBucketProvider, CancellationToken cancellationToken = default)
    {
        IBucket bucket = await namedBucketProvider.GetBucketAsync().ConfigureAwait(false);

        var context = new BucketContext(bucket);

        IQueryable<Airline> airlines = context.Query<Airline>();

        return airlines;
    }
}