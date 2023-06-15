namespace CouchGraphQl.GraphQl.Shared;

using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[UsedImplicitly]
internal sealed record InternalIdResolver<TId>
    where TId : IComparable
{
    private InternalIdResolver()
    {
    }

    public static string GetInternalId([Parent] IEntityWithId<TId> entityWithId)
    {
        return entityWithId.Id.ToString() ?? string.Empty;
    }
}