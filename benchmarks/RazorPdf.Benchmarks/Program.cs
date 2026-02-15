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

    [GlobalSetup]
    public async Task SetupAsync()
    {
        Microsoft.Playwright.Program.Main(["install", "chromium"]);

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
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
        var page = await _browser!.NewPageAsync();
        await page.SetContentAsync(_html);

        var pdf = await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4"
        });

        await page.CloseAsync();
        return pdf;
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
