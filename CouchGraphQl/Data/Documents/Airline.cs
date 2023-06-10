namespace CouchGraphQl.Data.Documents;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

using Newtonsoft.Json;

[PublicAPI]
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
        this.Type = createInput.Type;
    }

    [Key]
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public int Id { get; set; }
}