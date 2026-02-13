using AngleSharp.Dom;

namespace RazorPdf.Parsing;

/// <summary>
/// Walks an AngleSharp DOM tree and dispatches calls to an <see cref="IHtmlNodeVisitor"/>.
/// </summary>
public static class HtmlDocumentWalker
{
    /// <summary>
    /// Walks all children of the given node, dispatching element and text nodes to the visitor.
    /// </summary>
    public static void Walk(INode root, IHtmlNodeVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(visitor);

        foreach (var child in root.ChildNodes)
        {
            WalkNode(child, visitor);
        }
    }

    private static void WalkNode(INode node, IHtmlNodeVisitor visitor)
    {
        switch (node)
        {
            case IText text:
                visitor.VisitText(text);
                break;

            case IElement element:
                visitor.EnterElement(element);
                foreach (var child in element.ChildNodes)
                    WalkNode(child, visitor);
                visitor.LeaveElement(element);
                break;

            default:
                // For other node types (comments, processing instructions, etc.)
                // just recurse into children.
                foreach (var child in node.ChildNodes)
                    WalkNode(child, visitor);
                break;
        }
    }
}
