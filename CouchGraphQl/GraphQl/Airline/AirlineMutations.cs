namespace CouchGraphQl.GraphQl.Airline;

using CouchGraphQl.Data;
using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[ExtendObjectType(OperationTypeNames.Mutation)]
[PublicAPI]
public class AirlineMutations
{
    public Task<CreateAirlinePayload> CreateAirlineAsync(
        [Service] AirlineAccessor airlineAccessor,
        AirlineCreateInput airlineCreateInput,
        CancellationToken cancellationToken)
    {
        return airlineAccessor.CreateAsync(airlineCreateInput, cancellationToken).ContinueWith(task => new CreateAirlinePayload { Airline = task.Result }, cancellationToken);
    }

    public Task<Airline?> Throw()
    {
        if (true)
        {
            throw new InvalidOperationException("this is a test");
        }
    }
}