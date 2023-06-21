namespace CouchGraphQl.GraphQl.Scope;

public record Scope(string Name, IEnumerable<string> Collections);