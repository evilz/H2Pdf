using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using MigraDocCore.Rendering;
using RazorPdf.Parsing;
using MigraDocDocument = MigraDocCore.DocumentObjectModel.Document;

namespace RazorPdf;

/// <summary>
/// Renders an HTML string to a PDF document.
/// <para>
/// Pipeline: HTML string → AngleSharp parse → CSS resolution → <see cref="IHtmlNodeVisitor"/> (Visitor pattern)
/// → <see cref="PdfDocumentModel"/> → <see cref="PdfDocumentModelRenderer"/> → MigraDoc <see cref="Document"/>.
/// </para>
/// </summary>
public class HtmlPdfRenderer
{
    private readonly ILogger<HtmlPdfRenderer>? _logger;

    public HtmlPdfRenderer(ILogger<HtmlPdfRenderer>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses an HTML string and produces a MigraDoc <see cref="Document"/>.
    /// </summary>
    public MigraDocDocument Render(string html, HtmlPdfOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(html);
        options ??= new HtmlPdfOptions();

        _logger?.LogInformation("Parsing HTML input ({Length} chars)", html.Length);

        var parser = new HtmlParser();
        var htmlDocument = parser.ParseDocument(html);

        var model = BuildModel(htmlDocument, options);
        return PdfDocumentModelRenderer.BuildDocument(model);
    }

    /// <summary>
    /// Parses an HTML string and produces a MigraDoc <see cref="Document"/> (async).
    /// </summary>
    public async Task<MigraDocDocument> RenderAsync(string html, HtmlPdfOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(html);
        options ??= new HtmlPdfOptions();

        _logger?.LogInformation("Parsing HTML input ({Length} chars)", html.Length);

        var parser = new HtmlParser();
        var htmlDocument = await parser.ParseDocumentAsync(html);

        var model = BuildModel(htmlDocument, options);
        return PdfDocumentModelRenderer.BuildDocument(model);
    }

    /// <summary>
    /// Saves a MigraDoc <see cref="Document"/> to a PDF file.
    /// </summary>
    public void SaveToPdf(MigraDocDocument document, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _logger?.LogInformation("Saving PDF to {FilePath}", filePath);

        var pdfRenderer = new PdfDocumentRenderer
        {
            Document = document
        };

        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save(filePath);

        _logger?.LogInformation("PDF saved successfully to {FilePath}", filePath);
    }

    /// <summary>
    /// Parses HTML and saves the result directly to a PDF file.
    /// </summary>
    public void RenderToFile(string html, string outputPath, HtmlPdfOptions? options = null)
    {
        var document = Render(html, options);
        SaveToPdf(document, outputPath);
    }

    /// <summary>
    /// Parses HTML and saves the result directly to a PDF file (async).
    /// </summary>
    public async Task RenderToFileAsync(string html, string outputPath, HtmlPdfOptions? options = null)
    {
        var document = await RenderAsync(html, options);
        SaveToPdf(document, outputPath);
    }

    // ══════════════════════════ Internal ══════════════════════════════════

    private PdfDocumentModel BuildModel(AngleSharp.Dom.IDocument htmlDocument, HtmlPdfOptions options)
    {
        // Extract CSS from <style> elements.
        var cssResolver = new CssStyleResolver();
        foreach (var styleElement in htmlDocument.QuerySelectorAll("style"))
        {
            cssResolver.Parse(styleElement.TextContent);
        }

        var visitor = new MigraDocVisitor(cssResolver, options.BasePath, options.ContentWidthCm);
        var root = htmlDocument.Body ?? (AngleSharp.Dom.INode)htmlDocument.DocumentElement;
        HtmlDocumentWalker.Walk(root, visitor);

        var model = visitor.GetResult();

        // Apply document-level settings from options.
        model.DefaultFontName = options.DefaultFontName;
        model.DefaultFontSize = options.DefaultFontSize;
        model.PageMarginPt = options.PageMarginPt;

        _logger?.LogInformation(
            "HTML converted: {Sections} section(s), {Blocks} block(s)",
            model.Sections.Count,
            model.Sections.Sum(s => s.Blocks.Count));

        return model;
    }
}
