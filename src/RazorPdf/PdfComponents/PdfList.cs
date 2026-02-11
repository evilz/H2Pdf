using Microsoft.AspNetCore.Components;

namespace RazorPdf.PdfComponents;

/// <summary>
/// Represents an ordered or unordered list.
/// </summary>
public class PdfList : PdfComponentBase
{
    /// <summary>
    /// When true, renders as an ordered (numbered) list.
    /// </summary>
    [Parameter]
    public bool Ordered { get; set; }

    protected override string ElementName => "PdfList";

    protected override IReadOnlyDictionary<string, object?>? GetAttributes()
    {
        return new Dictionary<string, object?> { ["Ordered"] = Ordered };
    }
}
