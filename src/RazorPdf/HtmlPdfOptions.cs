namespace RazorPdf;

/// <summary>
/// Options for HTML-to-PDF rendering.
/// </summary>
public sealed class HtmlPdfOptions
{
    /// <summary>
    /// Base path for resolving relative image URLs.
    /// </summary>
    public string? BasePath { get; set; }

    /// <summary>
    /// Default font family to use when not specified in CSS.
    /// </summary>
    public string DefaultFontName { get; set; } = "Arial";

    /// <summary>
    /// Default font size in points when not specified in CSS.
    /// </summary>
    public double DefaultFontSize { get; set; } = 12;

    /// <summary>
    /// Content width of the PDF page in centimeters.
    /// Default is approximately A4 with 30pt margins.
    /// </summary>
    public double ContentWidthCm { get; set; } = 18.88;

    /// <summary>
    /// Page margin in points (applied to all sides).
    /// </summary>
    public double PageMarginPt { get; set; } = 30;
}
