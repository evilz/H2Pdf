using AngleSharp.Dom;

namespace H2Pdf.Parsing;

/// <summary>
/// Visitor interface for traversing an HTML DOM tree.
/// The walker invokes <see cref="EnterElement"/>/<see cref="LeaveElement"/> for element nodes
/// and <see cref="VisitText"/> for text nodes.
/// </summary>
public interface IHtmlNodeVisitor
{
    /// <summary>Called when entering an HTML element (before visiting its children).</summary>
    void EnterElement(IElement element);

    /// <summary>Called when leaving an HTML element (after visiting its children).</summary>
    void LeaveElement(IElement element);

    /// <summary>Called when visiting a text node.</summary>
    void VisitText(IText text);
}
