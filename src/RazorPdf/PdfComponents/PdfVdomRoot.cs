using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorPdf.PdfVdom;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Internal wrapper component that provides the PdfVdomBuilder as a CascadingValue
/// to the target PDF component. Used by PdfRenderer to bootstrap the component tree.
/// </summary>
internal class PdfVdomRoot : ComponentBase
{
    [Parameter]
    public PdfVdomBuilder? Builder { get; set; }

    [Parameter]
    public Type? ComponentType { get; set; }

    [Parameter]
    public IDictionary<string, object?>? ComponentParameters { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Builder == null || ComponentType == null) return;

        builder.OpenComponent<CascadingValue<PdfVdomBuilder>>(0);
        builder.AddComponentParameter(1, "Value", Builder);
        builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenComponent(0, ComponentType);
            if (ComponentParameters != null)
            {
                var seq = 1;
                foreach (var kvp in ComponentParameters)
                {
                    childBuilder.AddComponentParameter(seq++, kvp.Key, kvp.Value);
                }
            }
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}

