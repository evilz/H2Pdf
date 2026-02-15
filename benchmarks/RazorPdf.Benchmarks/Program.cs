// ------------------------------------------------------------
// Benchmark: HTML -> PDF pipeline comparisons
//   1) Playwright (Chromium) HTML rendering + PDF generation
//   2) MigraDocCore via a custom HtmlPdfRenderer (HTML -> MigraDoc DOM -> PDF)
// ------------------------------------------------------------
//
// Notes / choices for benchmark quality:
// - We keep the expensive Chromium process alive across iterations (GlobalSetup).
// - We create a BrowserContext + Page once and reuse them to reduce noise.
// - We "prime" both pipelines once in GlobalSetup so the first-call caches/JIT/font discovery
//   don't pollute the first measured iteration.
// - We avoid returning byte[] from benchmarks to reduce BDN-specific handling and keep
//   measurements closer to "time + allocations done by the pipeline itself".
//   (Allocations still happen; we just store results into a private sink.)
// - We specify PDF options (PrintBackground + margins) to make output more deterministic.
//
// Run: dotnet run -c Release
// ------------------------------------------------------------

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Playwright;
using MigraDocCore.Rendering;
using RazorPdf;

BenchmarkRunner.Run<PdfPipelineBenchmarks>();

[MemoryDiagnoser] // Captures allocations (Gen0/1/2, bytes allocated) per benchmark.
public class PdfPipelineBenchmarks
{
    private readonly HtmlPdfRenderer _htmlPdfRenderer = new();
    
    // Simple input document. In real cases, consider multiple payload sizes.
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

    // Playwright resources (created once in GlobalSetup, disposed in GlobalCleanup).
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    
    // "Sinks" to keep results alive and avoid any risk of dead-code elimination.
    // Also avoids returning byte[] from the benchmark methods (less noise).
    private byte[]? _playwrightPdfSink;
    private byte[]? _migraDocPdfSink;
    
    // Playwright PDF options:
    // - PrintBackground: invoices often rely on CSS background colors
    // - Margins: make output more deterministic
    // - Format: A4 for EU invoices
    private static readonly PagePdfOptions PdfOptions = new()
    {
        Format = "A4",
        PrintBackground = true,
        Margin = new() { Top = "12mm", Bottom = "12mm", Left = "12mm", Right = "12mm" }
    };

    [GlobalSetup]
    public async Task SetupAsync()
    {
        // Create Playwright driver (starts the Playwright connection).
        _playwright = await Playwright.CreateAsync();
        
        // Launch Chromium once. Launching per-iteration would dominate the benchmark.
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
        
        // Create a context once. Context is the recommended isolation unit in Playwright.
        // You can set locale/viewport here for better determinism if needed.
        _context = await _browser.NewContextAsync(new()
        {
            // If you care about determinism:
            Locale = "en-US",
            ViewportSize = new() { Width = 1280, Height = 720 }
        });

        // Create one page and reuse it.
        _page = await _browser.NewPageAsync();
        
        // ------------------------------------------------------------
        // Prime / warm-up both pipelines once:
        // This reduces "first call" effects (JIT, font discovery, internal caches).
        // BenchmarkDotNet does warmups too, but priming here often reduces variance
        // for browser-based pipelines.
        // ------------------------------------------------------------

        // Prime Playwright:
        await _page.SetContentAsync(_html);
        _playwrightPdfSink = await _page.PdfAsync(PdfOptions);

        // Prime MigraDoc pipeline:
        _migraDocPdfSink = RenderMigraDocToPdfBytes(_html);
    }

    [GlobalCleanup]
    public async Task CleanupAsync()
    {
        if (_context is not null) await _context.CloseAsync();
        if (_browser is not null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    // ------------------------------------------------------------
    // Benchmark 1: Playwright (Chromium) HTML -> PDF
    // ------------------------------------------------------------
    [Benchmark]
    public async Task<byte[]> PlaywrightHtmlToPdfAsync()
    {
        // Fail fast if setup did not run properly.
        if (_page is null)
        {
            throw new InvalidOperationException("Playwright page is not initialized. Ensure GlobalSetup completed successfully.");
        }
        
        // Load HTML into the page (DOM + CSS parsing + layout).
        await _page.SetContentAsync(_html);

        // Generate PDF bytes.
        return await _page.PdfAsync(PdfOptions);
    }

    // ------------------------------------------------------------
    // Benchmark 2: MigraDoc (via HtmlPdfRenderer) HTML -> MigraDoc -> PDF
    // ------------------------------------------------------------
    [Benchmark]
    public void MigraDoc_HtmlToPdf()
    {
        _migraDocPdfSink = RenderMigraDocToPdfBytes(_html);
    }

    // ------------------------------------------------------------
    // Helper: HTML -> MigraDoc document -> PDF bytes
    // Keeping this as a method makes the benchmark body cleaner.
    // ------------------------------------------------------------
    private byte[] RenderMigraDocToPdfBytes(string html)
    {
        // Convert HTML into a MigraDoc Document (your library responsibility).
        var document = _htmlPdfRenderer.Render(html);

        // Render MigraDoc Document into a PDFDocument.
        // NOTE: If fonts differ per machine, output can vary.
        var pdfRenderer = new PdfDocumentRenderer
        {
            Document = document
        };

        // Layout + render.
        pdfRenderer.RenderDocument();

        // Serialize into bytes.
        using var stream = new MemoryStream();
        pdfRenderer.PdfDocument.Save(stream, closeStream: false);
        return stream.ToArray();
    }
}
