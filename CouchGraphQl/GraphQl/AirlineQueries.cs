namespace CouchGraphQl.GraphQl;

using System.Text.Json;

using CouchGraphQl.Data;
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
    public IQueryable<Airline> GetAirlines([Service] MyBucketContext bucketContext)
    {
        IQueryable<Airline> query = bucketContext.Airlines
            .OrderByDescending(airline => airline.Name != null)
            .ThenBy(airline => airline.Name);

#if DEBUG
        string s = JsonSerializer.Serialize(query.FirstOrDefault());
        this.logger.LogDebug(AirlineQueries.MessageTemplate, s);
#endif
        return query;
    }
}