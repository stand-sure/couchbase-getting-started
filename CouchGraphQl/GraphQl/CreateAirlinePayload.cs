namespace CouchGraphQl.GraphQl;

using CouchGraphQl.Data.Documents;

using JetBrains.Annotations;

[PublicAPI]
public class CreateAirlinePayload
{
    /// <summary>
    /// Gets or sets the Airline.
    /// </summary>
    public Airline? Airline { get; set; }
}