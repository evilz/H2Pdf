namespace RazorPdf.PdfVdom;

/// <summary>
/// Base class for all PDF VDOM nodes
/// </summary>
public abstract class VNode
{
    public VNode? Parent { get; internal set; }
}
