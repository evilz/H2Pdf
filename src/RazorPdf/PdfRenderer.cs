using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using RazorPdf.PdfComponents;
using RazorPdf.PdfVdom;
using RazorPdf.Translation;

namespace RazorPdf;

/// <summary>
/// Renders Razor components to PDF documents using MigraDoc.
/// Components use the PDF VDOM pipeline: Razor components → VDOM tree → MigraDoc Document.
/// </summary>
public class PdfRenderer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PdfRenderer>? _logger;

    public PdfRenderer(IServiceProvider serviceProvider, ILogger<PdfRenderer>? logger = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger;
    }

    /// <summary>
    /// Renders a Razor component to a MigraDoc Document using the VDOM pipeline
    /// </summary>
    /// <typeparam name="TComponent">The Razor component type to render</typeparam>
    /// <param name="parameters">Parameters to pass to the component</param>
    /// <param name="options">PDF rendering options</param>
    /// <returns>A MigraDoc Document object</returns>
    public async Task<Document> RenderToDocumentAsync<TComponent>(
        IDictionary<string, object?>? parameters = null,
        PdfRenderOptions? options = null) 
        where TComponent : IComponent
    {
        _logger?.LogInformation("Starting PDF rendering for component {ComponentType}", typeof(TComponent).Name);

        var builder = new PdfVdomBuilder();

        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _serviceProvider.GetRequiredService<ILoggerFactory>());

        await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var rootParameters = new Dictionary<string, object?>
            {
                ["Builder"] = builder,
                ["ComponentType"] = typeof(TComponent),
                ["ComponentParameters"] = parameters
            };

            var renderParameters = ParameterView.FromDictionary(rootParameters);
            await htmlRenderer.RenderComponentAsync<PdfVdomRoot>(renderParameters);
        });

        _logger?.LogDebug("Component tree rendered, building VDOM");

        var root = builder.Build();

        // Translate VDOM to MigraDoc
        var translator = new VdomTranslator();
        var document = translator.Translate(root, options);

        _logger?.LogInformation("PDF document created successfully");

        return document;
    }

    /// <summary>
    /// Renders a Razor component to a MigraDoc Document (backward-compatible alias)
    /// </summary>
    public async Task<Document> RenderToPdfAsync<TComponent>(
        IDictionary<string, object?>? parameters = null,
        PdfRenderOptions? options = null)
        where TComponent : IComponent
    {
        return await RenderToDocumentAsync<TComponent>(parameters, options);
    }

    /// <summary>
    /// Renders a Razor component to PDF bytes
    /// </summary>
    /// <typeparam name="TComponent">The Razor component type to render</typeparam>
    /// <param name="parameters">Parameters to pass to the component</param>
    /// <param name="options">PDF rendering options</param>
    /// <returns>PDF file contents as a byte array</returns>
    public async Task<byte[]> RenderToPdfBytesAsync<TComponent>(
        IDictionary<string, object?>? parameters = null,
        PdfRenderOptions? options = null)
        where TComponent : IComponent
    {
        var document = await RenderToDocumentAsync<TComponent>(parameters, options);

        var pdfRenderer = new PdfDocumentRenderer
        {
            Document = document
        };

        pdfRenderer.RenderDocument();

        using var stream = new MemoryStream();
        pdfRenderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    /// <summary>
    /// Saves a MigraDoc document to a PDF file
    /// </summary>
    /// <param name="document">The document to save</param>
    /// <param name="filePath">The file path where the PDF should be saved</param>
    public void SaveToPdf(Document document, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        
        // Ensure directory exists
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
        
        _logger?.LogInformation("PDF saved successfully");
    }
}
