namespace RazorPdf.PdfComponents;

/// <summary>
/// Inserts a line break in the PDF document.
/// </summary>
public class PdfLineBreak : PdfComponentBase
{
    protected override string ElementName => "PdfLineBreak";
    protected override bool IsSelfClosing => true;
}
