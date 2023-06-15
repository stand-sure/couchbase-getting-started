namespace CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[PublicAPI]
public class AirlineAccessor : AccessorBase<Airline, AirlineCreateInput>
{
    public AirlineAccessor(MyBucketContext bucketContext, ILogger<AirlineAccessor> logger) : base(bucketContext, logger)
    {
    }

    protected override Airline MakeEntity(int id, AirlineCreateInput createInfo)
    {
        return new Airline(id, createInfo);
    }
}