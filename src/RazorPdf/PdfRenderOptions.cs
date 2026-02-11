using MigraDocCore.DocumentObjectModel;

namespace RazorPdf;

/// <summary>
/// Options for configuring PDF rendering
/// </summary>
public class PdfRenderOptions
{
    /// <summary>
    /// Page size (default: A4)
    /// </summary>
    public PageFormat PageSize { get; set; } = PageFormat.A4;

    /// <summary>
    /// Page orientation (default: Portrait)
    /// </summary>
    public Orientation PageOrientation { get; set; } = Orientation.Portrait;

    /// <summary>
    /// Top margin in centimeters (default: 2.5)
    /// </summary>
    public double MarginTop { get; set; } = 2.5;

    /// <summary>
    /// Bottom margin in centimeters (default: 2.0)
    /// </summary>
    public double MarginBottom { get; set; } = 2.0;

    /// <summary>
    /// Left margin in centimeters (default: 2.5)
    /// </summary>
    public double MarginLeft { get; set; } = 2.5;

    /// <summary>
    /// Right margin in centimeters (default: 2.5)
    /// </summary>
    public double MarginRight { get; set; } = 2.5;

    /// <summary>
    /// Default font name (default: "Arial")
    /// </summary>
    public string DefaultFont { get; set; } = "Arial";

    /// <summary>
    /// Default font size in points (default: 10)
    /// </summary>
    public double DefaultFontSize { get; set; } = 10;

    /// <summary>
    /// Allowed base directory for image file paths. 
    /// If null, file-based images are not allowed.
    /// </summary>
    public string? ImageAllowlistDirectory { get; set; }

    /// <summary>
    /// Default table border width in points (default: 0.5)
    /// </summary>
    public double TableBorderWidth { get; set; } = 0.5;
}
