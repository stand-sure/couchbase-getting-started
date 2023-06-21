namespace CouchGraphQl.Data;

using System.Reflection;

using Couchbase.KeyValue;
using Couchbase.Linq;

using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[PublicAPI]
public abstract class AccessorBase<T, TCreate>
    where T : TCreate, IEntityWithId<int>
{
    private const string MessageTemplate = $"{nameof(AccessorBase<T, TCreate>)}: {{Message}}";

    private readonly MyBucketContext bucketContext;
    private readonly ILogger<AccessorBase<T, TCreate>> logger;

    protected AccessorBase(MyBucketContext bucketContext, ILogger<AccessorBase<T, TCreate>> logger)
    {
        this.bucketContext = bucketContext ?? throw new ArgumentNullException(nameof(bucketContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IQueryable<T> Queryable => this.bucketContext.Query<T>(BucketQueryOptions.None).Where(entity => entity.Type == GetTypeName());

    public async Task<T?> CreateAsync(TCreate createInfo, CancellationToken cancellationToken = default)
    {
        (int id, string? key) = GenerateIdAndKey();

        T entity = this.MakeEntity(id, createInfo);
        entity.Type = string.IsNullOrEmpty(entity.Type) ? GetTypeName() : entity.Type;

        (string? scope, string? collectionName) = typeof(T).GetCustomAttribute<CouchbaseCollectionAttribute>()!;
         
        IScope scopeAsync = await this.bucketContext.Bucket.ScopeAsync(scope ?? "_default");
        
        ICouchbaseCollection collection = await scopeAsync.CollectionAsync(collectionName).ConfigureAwait(false);

        try
        {
            await collection.InsertAsync(key,
                entity,
                options =>
                {
                    options.CancellationToken(cancellationToken);
                    options.Durability(DurabilityLevel.MajorityAndPersistToActive);
                });
        }
        catch (Exception e)
        {
            this.logger.LogError(e, MessageTemplate, e.Message);
            throw;
        }

        return await GetByKeyAsync(collection, key).ConfigureAwait(false);
    }

    protected abstract T MakeEntity(int id, TCreate createInfo);

    private static (int id, string key) GenerateIdAndKey()
    {
        int id = Math.Abs(BitConverter.ToInt32(Guid.NewGuid().ToByteArray()));
        string key = $"{typeof(T).Name}_{id}".ToLowerInvariant();

        return (id, key);
    }

    private static Task<T?> GetByKeyAsync(ICouchbaseCollection collection, string key)
    {
        static T? GetResult(Task<IGetResult> task) =>
            task.Result.ContentAs<T>();

        return collection.GetAsync(key).ContinueWith(GetResult);
    }

    private static string GetTypeName() => typeof(T).Name.ToLowerInvariant();
}