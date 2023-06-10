namespace CouchGraphQl.GraphQl;

using CouchGraphQl.Data.Documents;

using HotChocolate.Data.Filters;

using JetBrains.Annotations;

[PublicAPI]
public class AirlineFilterInput : FilterInputType<Airline>
{
    protected override void Configure(IFilterInputTypeDescriptor<Airline> descriptor)
    {
        descriptor.Name(this.GetType().Name);
        descriptor.Field(airline => airline.Id).Name("iid");
        descriptor.Field(airline => airline.Type).Ignore();
        base.Configure(descriptor);
    }
}