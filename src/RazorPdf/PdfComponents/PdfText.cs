using Microsoft.AspNetCore.Components;
using RazorPdf.PdfVdom;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Adds a text node to the PDF VDOM.
/// </summary>
public class PdfText : ComponentBase
{
    [CascadingParameter]
    public PdfVdomBuilder? Builder { get; set; }

    /// <summary>
    /// The text content to add.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = "";

    protected override void OnInitialized()
    {
        if (Builder != null && !string.IsNullOrEmpty(Value))
        {
            Builder.AddText(Value);
        }
    }
}
