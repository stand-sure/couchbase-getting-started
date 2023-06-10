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

    private readonly ILogger<AirlineMutations> logger;

    public AirlineMutations(ILogger<AirlineMutations> logger)
    {
        this.logger = logger;
    }

    public async Task<Airline?> CreateAirlineAsync(
        [Service] MyBucketContext bucketContext,
        AirlineCreateInput airlineCreateInput)
    {
        int id = Math.Abs(BitConverter.ToInt32(Guid.NewGuid().ToByteArray()));
        string key = $"{nameof(Airline)}_{id}".ToLowerInvariant();
        
        var airline = new Airline(id, airlineCreateInput);
        
        ICouchbaseCollection collection = bucketContext.Airlines.Collection;
        
        _ = await collection.UpsertAsync(key, airline);

        Airline? a = await GetAirlineAsync(collection, key);

        this.logger.LogDebug(AirlineMutations.MessageTemplate, a?.ToString());

        return a;
    }

    private static Task<Airline?> GetAirlineAsync(ICouchbaseCollection collection, string key)
    {
        static Airline? GetResult(Task<IGetResult> task) => 
            task.Result.ContentAs<Airline>();

        return collection.GetAsync(key).ContinueWith(GetResult);
    }
}