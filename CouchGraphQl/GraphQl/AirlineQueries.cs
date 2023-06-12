namespace CouchGraphQl.GraphQl;

using CouchGraphQl.Data;
using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[PublicAPI]
[ExtendObjectType(OperationTypeNames.Query)]
public class AirlineQueries
{
    [UsePaging]
    [UseFiltering(typeof(AirlineFilterInput))]
    [UseSorting(typeof(AirlineSortInput))]
    public IQueryable<Airline> GetAirlines(
        [Service] AirlineAccessor airlineAccessor)
    {
        return airlineAccessor.Queryable;
    }
}