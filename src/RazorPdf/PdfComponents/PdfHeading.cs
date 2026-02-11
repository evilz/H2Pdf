using Microsoft.AspNetCore.Components;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Represents a heading in the PDF document.
/// </summary>
public class PdfHeading : PdfComponentBase
{
    /// <summary>
    /// Heading level (1–6).
    /// </summary>
    [Parameter]
    public int Level { get; set; } = 1;

    protected override string ElementName => "PdfHeading";

    protected override IReadOnlyDictionary<string, object?>? GetAttributes()
    {
        return new Dictionary<string, object?> { ["Level"] = Level };
    }
}
