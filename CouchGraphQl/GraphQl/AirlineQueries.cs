namespace CouchGraphQl.GraphQl;

using System.Text.Json;

using CouchGraphQl.Data;
using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[PublicAPI]
[ExtendObjectType(OperationTypeNames.Query)]
public class AirlineQueries
{
    private const string MessageTemplate = $"{nameof(AirlineQueries)}: {{Message}}";

    [UsePaging]
    [UseFiltering(typeof(AirlineFilterInput))]
    [UseSorting(typeof(AirlineSortInput))]
    public IQueryable<Airline> GetAirlines(
        [Service] MyBucketContext bucketContext,
        [Service] ILogger<AirlineQueries> logger)
    {
        IQueryable<Airline> query = bucketContext.Airlines
            .OrderByDescending(airline => airline.Name != null)
            .ThenBy(airline => airline.Name);

#if DEBUG
        string s = JsonSerializer.Serialize(query.FirstOrDefault());
        logger.LogDebug(AirlineQueries.MessageTemplate, s);
#endif
        return query;
    }
}