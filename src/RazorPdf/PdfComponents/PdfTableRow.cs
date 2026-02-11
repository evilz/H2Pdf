using Microsoft.AspNetCore.Components;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Represents a row in a PdfTable.
/// </summary>
public class PdfTableRow : PdfComponentBase
{
    /// <summary>
    /// When true, marks this row as a header row.
    /// </summary>
    [Parameter]
    public bool IsHeader { get; set; }

    protected override string ElementName => "PdfTableRow";

    protected override IReadOnlyDictionary<string, object?>? GetAttributes()
    {
        return new Dictionary<string, object?> { ["IsHeader"] = IsHeader };
    }
}
