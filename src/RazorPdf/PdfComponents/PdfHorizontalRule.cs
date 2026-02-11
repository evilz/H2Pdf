namespace RazorPdf.PdfComponents;

/// <summary>
/// Inserts a horizontal rule in the PDF document.
/// </summary>
public class PdfHorizontalRule : PdfComponentBase
{
    protected override string ElementName => "PdfHorizontalRule";
    protected override bool IsSelfClosing => true;
}
