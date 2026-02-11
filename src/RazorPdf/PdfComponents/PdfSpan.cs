using Microsoft.AspNetCore.Components;

namespace RazorPdf.PdfComponents;

/// <summary>
/// An inline container that can carry style attributes.
/// </summary>
public class PdfSpan : PdfComponentBase
{
    /// <summary>
    /// Optional style string for the span.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    protected override string ElementName => "PdfSpan";

    protected override IReadOnlyDictionary<string, object?>? GetAttributes()
    {
        if (Style != null)
            return new Dictionary<string, object?> { ["Style"] = Style };
        return null;
    }
}
