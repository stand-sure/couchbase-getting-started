namespace CouchGraphQl.Vault;

using System.Text.Json;

using Serilog;

internal class DictionaryFlattener
{
    private readonly Stack<string> context = new();
    private readonly IDictionary<string, string> data = new Dictionary<string, string>();
    private string currentPath = null!;

    public IDictionary<string, string> Flatten(IDictionary<string, object> dictionary)
    {
        this.ProcessDictionary(dictionary);
        return this.data;
    }

    private void EnterContext(string key)
    {
        this.context.Push(key);
        this.currentPath = string.Join(':', this.context.Reverse());
    }

    private void ExitContext()
    {
        this.context.Pop();
        this.currentPath = string.Join(':', this.context.Reverse());
    }

    private void ProcessDictionary(IDictionary<string, object> dictionary)
    {
        foreach ((string? key, object? value) in dictionary)
        {
            this.VisitNode(key, value);
        }
    }

    private void ProcessJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                this.StoreString(element.GetString());
                break;
            case JsonValueKind.Object:
                this.ProcessJsonObject(element);
                break;
            default:
                Log.Warning("Unknown: {Element}", element);
                this.StoreString(element.ToString());
                break;
        }
    }

    private void ProcessJsonObject(JsonElement element)
    {
        var dictionary = element.Deserialize<IDictionary<string, object>>();

        switch (dictionary?.Any())
        {
            case true:
                this.ProcessDictionary(dictionary);
                break;
            default:
                this.StoreString(element.ToString());
                break;
        }
    }

    private void StoreString(string? value)
    {
        this.data[this.currentPath] = value ?? string.Empty;
    }

    private void VisitNode(string entryKey, object node)
    {
        this.EnterContext(entryKey);

        switch (node)
        {
            case string text:
                this.StoreString(text);
                break;
            case IDictionary<string, object> dictionary:
            {
                this.ProcessDictionary(dictionary);
                break;
            }
            case JsonElement element:
                this.ProcessJsonElement(element);
                break;
            default:
                this.StoreString(node.ToString());
                break;
        }

        this.ExitContext();
    }
}