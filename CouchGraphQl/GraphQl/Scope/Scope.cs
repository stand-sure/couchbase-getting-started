namespace CouchGraphQl.GraphQl.Scope;

using JetBrains.Annotations;

[PublicAPI]
public record Scope(string Name, IEnumerable<string> Collections);