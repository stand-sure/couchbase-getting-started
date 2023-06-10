namespace CouchGraphQl.Data.Documents;

using System.Text.Json.Serialization;

using JetBrains.Annotations;

[PublicAPI]
public class AirlineCreateInput
{
    [JsonPropertyName("callsign")]
    public string? Callsign { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("iata")]
    public string? Iata { get; set; }

    [JsonPropertyName("icao")]
    public string? Icao { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}