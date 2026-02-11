using Microsoft.AspNetCore.Components;
using RazorPdf.PdfVdom;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Inserts an image into the PDF document. Self-closing element.
/// </summary>
public class PdfImage : ComponentBase
{
    [CascadingParameter]
    public PdfVdomBuilder? Builder { get; set; }

    /// <summary>
    /// Image source: file path string or byte[].
    /// </summary>
    [Parameter]
    public object? Source { get; set; }

    /// <summary>
    /// Optional image width.
    /// </summary>
    [Parameter]
    public double? Width { get; set; }

    /// <summary>
    /// Optional image height.
    /// </summary>
    [Parameter]
    public double? Height { get; set; }

    /// <summary>
    /// Alternative text for the image.
    /// </summary>
    [Parameter]
    public string? Alt { get; set; }

    protected override void OnInitialized()
    {
        if (Builder == null) return;

        var attrs = new Dictionary<string, object?>
        {
            ["Source"] = Source,
            ["Alt"] = Alt ?? ""
        };

        if (Width.HasValue) attrs["Width"] = Width.Value;
        if (Height.HasValue) attrs["Height"] = Height.Value;

        Builder.AddSelfClosingElement("PdfImage", attrs);
    }
}
