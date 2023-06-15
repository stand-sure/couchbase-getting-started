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
        this.VisitDictionary(dictionary);
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

    private void VisitDictionary(IDictionary<string, object> dictionary)
    {
        foreach ((string? key, object? value) in dictionary)
        {
            this.VisitNode(key, value);
        }
    }

    private void VisitJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                this.data[this.currentPath] = element.GetString() ?? string.Empty;
                break;
            case JsonValueKind.Object:
                this.VisitJsonObject(element);
                break;
            default:
                Log.Debug("Unknown: {Element}", element);
                break;
        }
    }

    private void VisitJsonObject(JsonElement element)
    {
        var dictionary = element.Deserialize<IDictionary<string, object>>();

        if (dictionary?.Any() is not true)
        {
            return;
        }

        this.VisitDictionary(dictionary);
    }

    private void VisitNode(string entryKey, object node)
    {
        this.EnterContext(entryKey);

        switch (node)
        {
            case string text:
                this.data[this.currentPath] = text;
                break;
            case IDictionary<string, object> dictionary:
            {
                this.VisitDictionary(dictionary);
                break;
            }
            case JsonElement element:
                this.VisitJsonElement(element);
                break;
            default:
                this.data[this.currentPath] = string.Empty;
                break;
        }

        this.ExitContext();
    }
}