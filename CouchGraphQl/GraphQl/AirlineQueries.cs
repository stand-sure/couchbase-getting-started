namespace CouchGraphQl.GraphQl;

using System.Text.Json;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;
using Couchbase.Linq.Metadata;

using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[PublicAPI]
[ExtendObjectType(OperationTypeNames.Query)]
public class AirlineQueries
{
    private readonly ILogger<AirlineQueries> logger;
    private const string MessageTemplate = $"{nameof(AirlineQueries)}: {{Message}}";

    /// <summary>
    /// Initializes a new instance of the <see cref="AirlineQueries"/> class.
    /// </summary>
    public AirlineQueries(ILogger<AirlineQueries> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [UsePaging]
    [UseFiltering(typeof(AirlineFilterInput))]
    [UseSorting(typeof(AirlineSortInput))]
    public async Task<IQueryable<Airline>> GetAirlines(
        [Service] INamedBucketProvider namedBucketProvider,
        CancellationToken cancellationToken = default)
    {
        IBucket bucket = await namedBucketProvider.GetBucketAsync().ConfigureAwait(false);

        var context = new BucketContext(bucket);

        IQueryable<Airline> query = context.Query<Airline>()
            .OrderByDescending(airline => airline.Name != null)
            .ThenBy(airline => airline.Name);

#if DEBUG
        string s = JsonSerializer.Serialize(query.FirstOrDefault());
        this.logger.LogDebug(AirlineQueries.MessageTemplate, s);
#endif
        return query;
    }
}