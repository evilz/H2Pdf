using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;

namespace H2Pdf;

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
        return await RenderToPdfAsync<TComponent>(null, parameters);
    }

    /// <summary>
    /// Renders a Razor component to a PDF document
    /// </summary>
    /// <typeparam name="TComponent">The Razor component type to render</typeparam>
    /// <param name="options">Rendering options for the PDF document</param>
    /// <param name="parameters">Parameters to pass to the component</param>
    /// <returns>A MigraDoc Document object</returns>
    public async Task<Document> RenderToPdfAsync<TComponent>(PdfRenderOptions? options, IDictionary<string, object?>? parameters = null)
        where TComponent : IComponent
    {
        _logger?.LogInformation("Starting PDF rendering for component {ComponentType}", typeof(TComponent).Name);

        // Create a renderer
        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _serviceProvider.GetRequiredService<ILoggerFactory>());
        var buildContext = new PdfBuildContext(options);
        var contextAccessor = _serviceProvider.GetRequiredService<PdfBuildContextAccessor>();

        // Render the component once to build the PDF model
        await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var renderParameters = parameters != null 
                ? ParameterView.FromDictionary(parameters) 
                : ParameterView.Empty;

            using var contextScope = contextAccessor.PushContext(buildContext);
            await htmlRenderer.RenderComponentAsync<TComponent>(renderParameters);
        });

        // Create PDF document
        var document = CreatePdfDocument(buildContext.Build());

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
    /// Creates a PDF document from a PDF document model
    /// </summary>
    private static Document CreatePdfDocument(PdfDocumentModel model)
    {
        return PdfDocumentModelRenderer.BuildDocument(model);
    }
}
