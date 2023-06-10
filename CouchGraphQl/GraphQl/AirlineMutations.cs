namespace CouchGraphQl.GraphQl;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;

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
        [Service] INamedBucketProvider namedBucketProvider,
        AirlineCreateInput airlineCreateInput)
    {
        var keyGuid = Guid.NewGuid();
        string key = keyGuid.ToString().ToLowerInvariant();
        var id = BitConverter.ToInt32(keyGuid.ToByteArray());
        var airline = new Airline(id, airlineCreateInput);
        
        ICouchbaseCollection collection = await GetCollection(namedBucketProvider).ConfigureAwait(false);
        _ = await collection.UpsertAsync(key, airline).ConfigureAwait(false);

        Airline? a = await GetAirlineAsync(collection, key);

        this.logger.LogDebug(AirlineMutations.MessageTemplate, a?.ToString());

        return a;
    }

    private static async Task<Airline?> GetAirlineAsync(ICouchbaseCollection collection, string key)
    {
        IGetResult getResult = await collection.GetAsync(key).ConfigureAwait(false);
        var airline = getResult.ContentAs<Airline>();

        return airline;
    }

    private static async Task<ICouchbaseCollection> GetCollection(INamedBucketProvider namedBucketProvider)
    {
        IBucket bucket = await namedBucketProvider.GetBucketAsync().ConfigureAwait(false);

        IScope scope = await bucket.ScopeAsync("inventory").ConfigureAwait(false);

        ICouchbaseCollection collection = await scope.CollectionAsync("airline").ConfigureAwait(false);

        return (collection);
    }
}