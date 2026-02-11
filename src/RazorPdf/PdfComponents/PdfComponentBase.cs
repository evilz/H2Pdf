using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorPdf.PdfVdom;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Base class for PDF VDOM components. Opens a VDOM element on initialization
/// and closes it on disposal. Children are rendered via ChildContent.
/// </summary>
public abstract class PdfComponentBase : ComponentBase, IDisposable
{
    [CascadingParameter]
    public PdfVdomBuilder? Builder { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The VDOM element name (e.g., "PdfParagraph").
    /// </summary>
    protected abstract string ElementName { get; }

    /// <summary>
    /// Override to provide element attributes/parameters.
    /// </summary>
    protected virtual IReadOnlyDictionary<string, object?>? GetAttributes() => null;

    /// <summary>
    /// Whether this is a self-closing element (no children).
    /// </summary>
    protected virtual bool IsSelfClosing => false;

    private bool _opened;

    protected override void OnInitialized()
    {
        if (Builder == null) return;

        if (IsSelfClosing)
        {
            Builder.AddSelfClosingElement(ElementName, GetAttributes());
        }
        else
        {
            Builder.OpenElement(ElementName, GetAttributes());
            _opened = true;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent != null && !IsSelfClosing)
        {
            builder.AddContent(0, ChildContent);
        }
    }

    public virtual void Dispose()
    {
        if (_opened && Builder != null)
        {
            Builder.CloseElement();
            _opened = false;
        }
    }
}
