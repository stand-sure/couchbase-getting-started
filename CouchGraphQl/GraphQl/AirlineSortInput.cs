namespace CouchGraphQl.GraphQl;

using CouchGraphQl.Data.Documents;

using HotChocolate.Data.Sorting;

public class AirlineSortInput : SortInputType<Airline>
{
    protected override void Configure(ISortInputTypeDescriptor<Airline> descriptor)
    {
        descriptor.Ignore(airline => airline.Type);
        base.Configure(descriptor);
    }
}