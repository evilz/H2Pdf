namespace RazorPdf.PdfVdom;

/// <summary>
/// Represents a text node in the PDF VDOM tree
/// </summary>
public class VText : VNode
{
    public string Text { get; }

    public VText(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }
}
