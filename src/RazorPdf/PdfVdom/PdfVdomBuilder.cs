namespace RazorPdf.PdfVdom;

/// <summary>
/// Stack-based builder for constructing PDF VDOM trees.
/// Used per-render invocation to build the document structure.
/// </summary>
public class PdfVdomBuilder
{
    private readonly Stack<VElement> _stack = new();
    private VElement? _root;

    /// <summary>
    /// Opens a new element and pushes it onto the stack
    /// </summary>
    public void OpenElement(string name, IReadOnlyDictionary<string, object?>? attributes = null)
    {
        var element = new VElement(name, attributes);

        if (_stack.Count > 0)
        {
            _stack.Peek().AddChild(element);
        }

        _stack.Push(element);
    }

    /// <summary>
    /// Adds a text node as a child of the current element
    /// </summary>
    public void AddText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (_stack.Count == 0)
            throw new InvalidOperationException("Cannot add text without an open element.");

        _stack.Peek().AddChild(new VText(text));
    }

    /// <summary>
    /// Closes the current element and pops it from the stack
    /// </summary>
    public void CloseElement()
    {
        if (_stack.Count == 0)
            throw new InvalidOperationException("No element to close.");

        var closed = _stack.Pop();

        if (_stack.Count == 0)
        {
            _root = closed;
        }
    }

    /// <summary>
    /// Adds a self-closing element (no children) to the current element
    /// </summary>
    public void AddSelfClosingElement(string name, IReadOnlyDictionary<string, object?>? attributes = null)
    {
        var element = new VElement(name, attributes);

        if (_stack.Count > 0)
        {
            _stack.Peek().AddChild(element);
        }
        else
        {
            _root = element;
        }
    }

    /// <summary>
    /// Returns the root VNode of the built tree
    /// </summary>
    public VNode Build()
    {
        if (_stack.Count > 0)
            throw new InvalidOperationException($"Unclosed elements remain: {string.Join(", ", _stack.Select(e => e.Name))}");

        return _root ?? throw new InvalidOperationException("No elements were added to the builder.");
    }
}
