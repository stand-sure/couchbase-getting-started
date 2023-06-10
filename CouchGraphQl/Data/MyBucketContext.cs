namespace CouchGraphQl.Data;

using Couchbase;
using Couchbase.Linq;

using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[PublicAPI]
public class MyBucketContext : BucketContext
{
    public IDocumentSet<Airline> Airlines { get; set; } = null!;

    public MyBucketContext(IBucket bucket) : base(bucket)
    {
    }
}