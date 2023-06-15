namespace CouchGraphQl.Data.Documents;

using System.Text.Json.Serialization;

using Couchbase.Linq;
using Couchbase.Linq.Filters;

using JetBrains.Annotations;

[PublicAPI]
[CouchbaseCollection("inventory", "airline")]
public class Airline : AirlineCreateInput, IEntityWithId<int>
{
    public Airline()
    {
    }

    public Airline(int id, AirlineCreateInput createInput)
    {
        this.Id = id;
        this.Callsign = createInput.Callsign;
        this.Country = createInput.Country;
        this.Iata = createInput.Iata;
        this.Icao = createInput.Icao;
        this.Name = createInput.Name;
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = nameof(Airline).ToLowerInvariant();
}