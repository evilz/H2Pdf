using Microsoft.AspNetCore.Components;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Represents a cell in a PdfTableRow.
/// </summary>
public class PdfTableCell : PdfComponentBase
{
    /// <summary>
    /// Number of columns this cell spans.
    /// </summary>
    [Parameter]
    public int ColSpan { get; set; } = 1;

    /// <summary>
    /// Number of rows this cell spans. Reserved for future use.
    /// </summary>
    [Parameter]
    public int? RowSpan { get; set; }

    protected override string ElementName => "PdfTableCell";

    protected override IReadOnlyDictionary<string, object?>? GetAttributes()
    {
        var attrs = new Dictionary<string, object?>
        {
            ["ColSpan"] = ColSpan
        };
        if (RowSpan.HasValue)
            attrs["RowSpan"] = RowSpan.Value;
        return attrs;
    }
}
