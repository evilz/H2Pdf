using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Playwright;
using MigraDocCore.Rendering;
using RazorPdf;

BenchmarkRunner.Run<PdfPipelineBenchmarks>();

[MemoryDiagnoser]
public class PdfPipelineBenchmarks
{
    private readonly HtmlPdfRenderer _htmlPdfRenderer = new();
    private readonly string _html = """
        <!doctype html>
        <html>
        <head>
            <style>
                body { font-family: Arial; }
                table { border-collapse: collapse; width: 100%; }
                th, td { border: 1px solid #ddd; padding: 8px; }
                th { background-color: #f2f2f2; }
            </style>
        </head>
        <body>
            <h1>Invoice #INV-1001</h1>
            <p>Customer: Demo User</p>
            <table>
                <thead>
                    <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
                </thead>
                <tbody>
                    <tr><td>Design</td><td>2</td><td>$120.00</td></tr>
                    <tr><td>Development</td><td>8</td><td>$800.00</td></tr>
                    <tr><td>Testing</td><td>3</td><td>$180.00</td></tr>
                </tbody>
            </table>
        </body>
        </html>
        """;

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _page = await _browser.NewPageAsync();
    }

    [GlobalCleanup]
    public async Task CleanupAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }

    [Benchmark]
    public async Task<byte[]> PlaywrightHtmlToPdfAsync()
    {
        if (_page is null)
        {
            throw new InvalidOperationException("Playwright page is not initialized. Ensure GlobalSetup completed successfully.");
        }

        await _page.SetContentAsync(_html);

        return await _page.PdfAsync(new PagePdfOptions
        {
            Format = "A4"
        });
    }

    [Benchmark]
    public byte[] HtmlToMigraDocPdf()
    {
        var document = _htmlPdfRenderer.Render(_html);
        var pdfRenderer = new PdfDocumentRenderer
        {
            Document = document
        };

        pdfRenderer.RenderDocument();

        using var stream = new MemoryStream();
        pdfRenderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }
}
