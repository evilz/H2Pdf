using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;

namespace RazorPdf;

/// <summary>
/// Renders Razor components to PDF documents using MigraDoc
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
    /// Renders a Razor component to a PDF document
    /// </summary>
    /// <typeparam name="TComponent">The Razor component type to render</typeparam>
    /// <param name="parameters">Parameters to pass to the component</param>
    /// <returns>A MigraDoc Document object</returns>
    public async Task<Document> RenderToPdfAsync<TComponent>(IDictionary<string, object?>? parameters = null) 
        where TComponent : IComponent
    {
        _logger?.LogInformation("Starting PDF rendering for component {ComponentType}", typeof(TComponent).Name);

        // Create a renderer
        await using var componentRenderer = new PdfComponentRenderer(
            _serviceProvider,
            _serviceProvider.GetRequiredService<ILoggerFactory>());

        // Render the component to VDOM
        var vdomNodes = await componentRenderer.RenderToVdomAsync<TComponent>(
            parameters != null ? ParameterView.FromDictionary(parameters) : ParameterView.Empty);

        _logger?.LogDebug("Component rendered to VDOM. Node count: {Count}", vdomNodes.Count);

        // Create PDF document
        var document = CreatePdfDocument(vdomNodes);

        _logger?.LogInformation("PDF document created successfully");

        return document;
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

    /// <summary>
    /// Creates a PDF document from VDOM content
    /// </summary>
    private Document CreatePdfDocument(IReadOnlyList<PdfVdomNode> nodes)
    {
        var document = new Document();
        var section = document.AddSection();
        
        // Convert VDOM to MigraDoc elements using the converter
        HtmlToPdfConverter.ConvertVdomToSection(nodes, section);
        
        return document;
    }
}
