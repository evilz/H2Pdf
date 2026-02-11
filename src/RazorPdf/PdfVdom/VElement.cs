namespace RazorPdf.PdfVdom;

/// <summary>
/// Represents a PDF element node in the VDOM tree (e.g., PdfParagraph, PdfTable)
/// </summary>
public class VElement : VNode
{
    public string Name { get; }
    public IReadOnlyDictionary<string, object?> Attributes { get; }
    private readonly List<VNode> _children = [];
    public IReadOnlyList<VNode> Children => _children;

    public VElement(string name, IReadOnlyDictionary<string, object?>? attributes = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Attributes = attributes ?? new Dictionary<string, object?>();
    }

    internal void AddChild(VNode child)
    {
        child.Parent = this;
        _children.Add(child);
    }

    /// <summary>
    /// Gets an attribute value by key, returning default if not found
    /// </summary>
    public T? GetAttribute<T>(string key, T? defaultValue = default)
    {
        if (Attributes.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }
}
