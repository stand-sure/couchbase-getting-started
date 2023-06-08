namespace StartUsingConsole.HostedServices;

using System.Text.Json;

using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Couchbase.Linq;
using Couchbase.Query;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StartUsingConsole.Documents;

public class CouchbaseDemoService : BackgroundService
{
    private const string MessageTemplate = $"{nameof(CouchbaseDemoService)}: {{Method}} {{Data}}";
    private const string ScopeName = "tenant_agent_00";
    private const string UsersCollection = "users";
    private readonly AsyncLazy<IBucket> bucketLazy;

    private readonly ILogger<CouchbaseDemoService> logger;

    public CouchbaseDemoService(ILogger<CouchbaseDemoService> logger, INamedBucketProvider namedBucketProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this.bucketLazy = new AsyncLazy<IBucket>(async () => await namedBucketProvider.GetBucketAsync());
    }

    private IBucket Bucket => this.bucketLazy.GetValue();

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation(CouchbaseDemoService.MessageTemplate, nameof(this.StopAsync), $"{nameof(this.StopAsync)} invoked");
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string documentKey = "my-document-key";

        _ = await this.UpsertDocumentAsync(documentKey, new { Name = "Ted", Age = 31 }).ConfigureAwait(false);
        _ = await this.GetDocumentAsync(documentKey).ConfigureAwait(false);
        _ = await this.DynamicQueryAsync("inventory", "SELECT * FROM airline WHERE id = 10").ConfigureAwait(false);
        
        await foreach (Airline airline in this.LinqQueryAsync().WithCancellation(stoppingToken).ConfigureAwait(false))
        {
            string data = JsonSerializer.Serialize(airline);
            this.logger.LogInformation(CouchbaseDemoService.MessageTemplate, nameof(this.LinqQueryAsync), data);
        }
    }

    private async IAsyncEnumerable<Airline> LinqQueryAsync()
    {
        var ctx = new BucketContext(this.Bucket);

        IQueryable<Airline> query = from airline in ctx.Query<Airline>()
            where airline.Country == "United Kingdom"
            select airline;

        IAsyncEnumerable<Airline> asyncEnumerable = query.Take(10).ToAsyncEnumerable();

        await foreach (Airline airline in asyncEnumerable.ConfigureAwait(false))
        {
            yield return airline;
        }
    }

    private async Task<IQueryResult<dynamic>> DynamicQueryAsync(string scopeName, string statement)
    {
        IScope inventoryScope = await this.Bucket.ScopeAsync(scopeName).ConfigureAwait(false);
        IQueryResult<dynamic> queryResult = await inventoryScope.QueryAsync<dynamic>(statement).ConfigureAwait(false);

        await foreach (dynamic row in queryResult)
        {
            string rowData = row.ToString();
            this.logger.LogInformation(CouchbaseDemoService.MessageTemplate, nameof(this.DynamicQueryAsync), rowData);
        }

        return queryResult;
    }

    private async Task<ICouchbaseCollection> GetCollection(string scopeName, string collectionName)
    {
        IScope scope = await this.Bucket.ScopeAsync(scopeName);
        ICouchbaseCollection collection = await scope.CollectionAsync(collectionName);

        return collection;
    }

    private async Task<IGetResult> GetDocumentAsync(string key)
    {
        ICouchbaseCollection collection =
            await this.GetCollection(CouchbaseDemoService.ScopeName, CouchbaseDemoService.UsersCollection);

        IGetResult getResult = await collection.GetAsync(key);

        string? getResultData = getResult.ContentAs<dynamic>()?.ToString();
        this.logger.LogInformation(CouchbaseDemoService.MessageTemplate, nameof(this.GetDocumentAsync), getResultData);

        return getResult;
    }

    private async Task<IMutationResult> UpsertDocumentAsync<T>(string key, T content)
    {
        ICouchbaseCollection collection =
            await this.GetCollection(CouchbaseDemoService.ScopeName, CouchbaseDemoService.UsersCollection);

        IMutationResult upsertResult = await collection.UpsertAsync(key, content);

        this.logger.LogInformation(CouchbaseDemoService.MessageTemplate, nameof(this.UpsertDocumentAsync), upsertResult);

        return upsertResult;
    }
}