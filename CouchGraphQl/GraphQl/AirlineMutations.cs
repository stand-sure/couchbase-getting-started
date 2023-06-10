namespace CouchGraphQl.GraphQl;

using Couchbase.KeyValue;

using CouchGraphQl.Data;
using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[ExtendObjectType(OperationTypeNames.Mutation)]
[PublicAPI]
public class AirlineMutations
{
    private const string MessageTemplate = $"{nameof(AirlineMutations)}: {{Message}}";

    public async Task<Airline?> CreateAirlineAsync(
        [Service] MyBucketContext bucketContext,
        [Service] ILogger<AirlineMutations> logger,
        AirlineCreateInput airlineCreateInput)
    {
        (int id, string? key) = GenerateIdAndKey();
        
        var airline = new Airline(id, airlineCreateInput);
        
        ICouchbaseCollection collection = bucketContext.Airlines.Collection;
        
        _ = await collection.UpsertAsync(key, airline);

        Airline? a = await GetAirlineAsync(collection, key);

        logger.LogDebug(AirlineMutations.MessageTemplate, a?.ToString());

        return a;
    }

    private static (int id, string key) GenerateIdAndKey()
    {
        int id = Math.Abs(BitConverter.ToInt32(Guid.NewGuid().ToByteArray()));
        string key = $"{nameof(Airline)}_{id}".ToLowerInvariant();

        return (id, key);
    }

    private static Task<Airline?> GetAirlineAsync(ICouchbaseCollection collection, string key)
    {
        static Airline? GetResult(Task<IGetResult> task) => 
            task.Result.ContentAs<Airline>();

        return collection.GetAsync(key).ContinueWith(GetResult);
    }
}