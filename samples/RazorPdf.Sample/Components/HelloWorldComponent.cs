using Microsoft.AspNetCore.Components;

namespace RazorPdf.Sample.Components;

/// <summary>
/// A simple Razor component for demonstration
/// </summary>
public class HelloWorldComponent : ComponentBase
{
    [Parameter]
    public string Name { get; set; } = "World";

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "greeting");
        
        builder.OpenElement(seq++, "h1");
        builder.AddContent(seq++, "Hello, RazorPdf!");
        builder.CloseElement();
        
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, $"Welcome {Name} to PDF generation with Razor components!");
        builder.CloseElement();
        
        builder.OpenElement(seq++, "p");
        builder.AddContent(seq++, "This content was generated from a Razor component and converted to PDF using MigraDoc.");
        builder.CloseElement();
        
        builder.CloseElement();
    }
}
