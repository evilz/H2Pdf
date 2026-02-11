using System.Collections.ObjectModel;

namespace RazorPdf;

internal abstract class PdfVdomNode
{
}

internal sealed class PdfVdomElement : PdfVdomNode
{
    public PdfVdomElement(
        string tagName,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IReadOnlyList<PdfVdomNode>? children = null)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("Tag name cannot be null or empty.", nameof(tagName));
        }

        TagName = tagName;
        Attributes = attributes ?? new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase));
        Children = children ?? Array.Empty<PdfVdomNode>();
    }

    public string TagName { get; }

    public IReadOnlyDictionary<string, object?> Attributes { get; }

    public IReadOnlyList<PdfVdomNode> Children { get; }
}

internal sealed class PdfVdomText : PdfVdomNode
{
    public PdfVdomText(string text)
    {
        Text = text ?? string.Empty;
    }

    public string Text { get; }
}
