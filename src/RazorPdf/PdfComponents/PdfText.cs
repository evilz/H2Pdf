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

    [CascadingParameter(Name = "ParentVElement")]
    public VElement? ParentElement { get; set; }

    /// <summary>
    /// The text content to add.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = "";

    protected override void OnInitialized()
    {
        if (Builder != null && ParentElement != null && !string.IsNullOrEmpty(Value))
        {
            Builder.AddText(Value, ParentElement);
        }
    }
}
