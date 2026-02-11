namespace RazorPdf.PdfVdom;

/// <summary>
/// Builder for constructing PDF VDOM trees via component registration.
/// Each component creates a node and registers it with its parent.
/// Thread-safe for single-threaded Blazor rendering.
/// </summary>
public class PdfVdomBuilder
{
    private VElement? _root;

    /// <summary>
    /// Creates a new element node and registers it as a child of the given parent.
    /// If parent is null, the element becomes the root.
    /// Returns the created element (to be cascaded to children).
    /// </summary>
    public VElement CreateElement(string name, IReadOnlyDictionary<string, object?>? attributes = null, VElement? parent = null)
    {
        var element = new VElement(name, attributes);

        if (parent != null)
        {
            parent.AddChild(element);
        }
        else if (_root == null)
        {
            _root = element;
        }

        return element;
    }

    /// <summary>
    /// Adds a text node as a child of the given parent element
    /// </summary>
    public void AddText(string text, VElement parent)
    {
        if (string.IsNullOrEmpty(text))
            return;

        parent.AddChild(new VText(text));
    }

    /// <summary>
    /// Returns the root VNode of the built tree
    /// </summary>
    public VNode Build()
    {
        return _root ?? throw new InvalidOperationException("No elements were added to the builder.");
    }
}
