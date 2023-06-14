namespace CouchGraphQl.GraphQl;

using System.Text.RegularExpressions;

using CouchGraphQl.Data.Documents;

public abstract class ObjectTypeWithId<T> : ObjectType<T>
    where T : IEntityWithId<int>
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        this.ConfigureImplementation(descriptor);
        descriptor.ImplementsNode().IdField(entityWithId => entityWithId.Id);

        descriptor.Field("iid")
            .Type<StringType>()
            .ResolveWith<InternalIdResolver<int>>(_ => InternalIdResolver<int>.GetInternalId(default!));

        descriptor.Field("key")
            .Type<StringType>()
            .ResolveWith<KeyResolver>(_ => KeyResolver.GetKey<T>(default!, default!));

        string name = this.GetType().Name;
        var regex = new Regex("(.*)?Type$");
        name = (regex.Match(name).Groups as IList<Group>).LastOrDefault()?.Value ?? name;
        descriptor.Name(name);
        
        base.Configure(descriptor);
    }

    protected virtual void ConfigureImplementation(IObjectTypeDescriptor<T> descriptor)
    {
    }
}