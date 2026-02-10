using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RazorPdf;
using RazorPdf.Sample.Components;

// Set up dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddRazorPdf();

var serviceProvider = services.BuildServiceProvider();

// Get the PDF renderer
var pdfRenderer = serviceProvider.GetRequiredService<PdfRenderer>();

Console.WriteLine("RazorPdf Sample - Generating PDF from Razor Components");
Console.WriteLine("=====================================================");

try
{
    // Render a component to PDF
    var parameters = new Dictionary<string, object?>
    {
        { "Name", "Developer" }
    };

    var document = await pdfRenderer.RenderToPdfAsync<HelloWorldComponent>(parameters);
    
    // Save to file
    var outputPath = "sample-output.pdf";
    pdfRenderer.SaveToPdf(document, outputPath);
    
    Console.WriteLine($"PDF generated successfully: {Path.GetFullPath(outputPath)}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error generating PDF: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
