using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

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
        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _serviceProvider.GetRequiredService<ILoggerFactory>());

        // Render the component to HTML
        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var renderParameters = parameters != null 
                ? ParameterView.FromDictionary(parameters) 
                : ParameterView.Empty;
            
            var output = await htmlRenderer.RenderComponentAsync<TComponent>(renderParameters);
            return output.ToHtmlString();
        });

        _logger?.LogDebug("Component rendered to HTML. Length: {Length}", html.Length);

        // Create PDF document
        var document = CreatePdfDocument(html);

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
    /// Creates a PDF document from HTML content
    /// </summary>
    private Document CreatePdfDocument(string html)
    {
        var document = new Document();
        var section = document.AddSection();
        
        // Parse and add HTML content
        // This is a simplified version - in a real implementation, 
        // you would parse the HTML and convert it to MigraDoc elements
        var paragraph = section.AddParagraph();
        paragraph.AddText(html);
        
        return document;
    }
}
