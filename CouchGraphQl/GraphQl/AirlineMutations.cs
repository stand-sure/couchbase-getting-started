namespace CouchGraphQl.GraphQl;

using CouchGraphQl.Data;
using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[ExtendObjectType(OperationTypeNames.Mutation)]
[PublicAPI]
public class AirlineMutations
{
   public Task<Airline?> CreateAirlineAsync(
        [Service] AirlineAccessor airlineAccessor,
        AirlineCreateInput airlineCreateInput)
    {
        return airlineAccessor.CreateAsync(airlineCreateInput);
    }
}