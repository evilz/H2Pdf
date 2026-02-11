using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorPdf.PdfVdom;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Base class for PDF VDOM components. Creates a VDOM element and cascades it
/// to child components, building a tree that mirrors the component hierarchy.
/// </summary>
public abstract class PdfComponentBase : ComponentBase
{
    [CascadingParameter]
    public PdfVdomBuilder? Builder { get; set; }

    [CascadingParameter(Name = "ParentVElement")]
    public VElement? ParentElement { get; set; }

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

    /// <summary>
    /// The VElement node created by this component, cascaded to children.
    /// </summary>
    protected VElement? CurrentElement { get; private set; }

    protected override void OnInitialized()
    {
        if (Builder == null) return;

        CurrentElement = Builder.CreateElement(ElementName, GetAttributes(), ParentElement);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent != null && !IsSelfClosing && CurrentElement != null)
        {
            // Cascade the current element as the parent for child components
            builder.OpenComponent<CascadingValue<VElement>>(0);
            builder.AddComponentParameter(1, "Name", "ParentVElement");
            builder.AddComponentParameter(2, "Value", CurrentElement);
            builder.AddComponentParameter(3, "ChildContent", ChildContent);
            builder.CloseComponent();
        }
    }
}
