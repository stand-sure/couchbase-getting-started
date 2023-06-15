namespace CouchGraphQl.GraphQl.Airline;

using CouchGraphQl.Data;
using CouchGraphQl.Data.Documents;
using CouchGraphQl.GraphQl.Shared;

using HotChocolate.Resolvers;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

[PublicAPI]
[ExtendObjectType(OperationTypeNames.Query)]
public class AirlineQueries
{
    [LoqQuery]
    [UsePaging]
    [UseFiltering(typeof(AirlineFilterInput))]
    [UseSorting(typeof(AirlineSortInput))]
    public IQueryable<Airline> GetAirlines(
        [Service] AirlineAccessor airlineAccessor)
    {
        return airlineAccessor.Queryable;
    }
}