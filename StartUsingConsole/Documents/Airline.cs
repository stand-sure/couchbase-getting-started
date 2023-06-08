namespace StartUsingConsole.Documents;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class Airline
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("iata")]
    public string? Iata { get; set; }

    [JsonPropertyName("icao")]
    public string? Icao { get; set; }

    [JsonPropertyName("callsign")]
    public string? Callsign { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}