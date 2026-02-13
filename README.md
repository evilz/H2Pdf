# RazorPdf

RazorPdf bridges the gap between modern UI development and PDF generation. It lets you create PDFs using Razor components while emitting a structured PDF document model directly. For HTML inputs, a separate `HtmlPdfRenderer` pipeline is available.

RazorPdf is a .NET framework that enables developers to use ASP.NET Core Razor components (typically used for web applications) to generate PDF documents. Components emit a PDF document model that is rendered deterministically to MigraDoc.

## Features

- **Razor Component Rendering**: Use familiar ASP.NET Core Razor components to emit PDF document models
- **Optional HTML Rendering**: Convert HTML strings via `HtmlPdfRenderer` when needed
- **Type-Safe**: Leverage C# and .NET type system for building PDF documents
- **Fluent API**: Build PDF documents programmatically with an intuitive fluent interface
- **Cross-Platform**: Works on Windows, Linux, and macOS thanks to PdfSharpCore and MigraDocCore
- **Dependency Injection Support**: Integrate seamlessly with ASP.NET Core DI container

## Installation

> **Note:** RazorPdf is not yet published to NuGet. Until it is available, you should build and reference it from source. See [Building from Source](#building-from-source) for instructions.

## Quick Start

### 1. Create a Razor Component

Create a `.razor` file (e.g., `HelloWorld.razor`) that emits PDF content through the build context:

```razor
@using Microsoft.AspNetCore.Components

@inject PdfBuildContextAccessor PdfContextAccessor

@code {
    [Parameter]
    public string Name { get; set; } = "World";

    [Parameter]
    public string? Message { get; set; }

    protected override void OnInitialized()
    {
        var pdfBuilder = PdfContextAccessor.GetRequiredContext().Builder;
        pdfBuilder.AddSection()
            .AddHeading("Hello, RazorPdf!", 1)
            .AddParagraph($"Welcome {Name} to PDF generation with Razor components!");

        if (!string.IsNullOrEmpty(Message))
        {
            pdfBuilder.AddParagraph(paragraph =>
                paragraph.AddText(Message, new PdfTextStyle { Bold = true }));
        }
    }
}
```

### 2. Configure Services

```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddRazorPdf();

var serviceProvider = services.BuildServiceProvider();
```

### 3. Render to PDF

```csharp
var pdfRenderer = serviceProvider.GetRequiredService<PdfRenderer>();

var parameters = new Dictionary<string, object?>
{
    { "Name", "Developer" },
    { "Message", "This is a real .razor file component!" }
};

var document = await pdfRenderer.RenderToPdfAsync<HelloWorld>(parameters);
pdfRenderer.SaveToPdf(document, "output.pdf");
```

### Optional: Render HTML directly

```csharp
var htmlRenderer = new HtmlPdfRenderer();
var htmlDocument = htmlRenderer.Render("""
    <h1>Hello HTML</h1>
    <p>This PDF was generated from HTML.</p>
""");
htmlRenderer.SaveToPdf(htmlDocument, "html-output.pdf");
```

## Building from Source

```bash
git clone https://github.com/evilz/RazorPdf.git
cd RazorPdf
dotnet build
```

## Running the Samples

The sample project demonstrates RazorPdf capabilities with two examples:

1. **HelloWorld** - A simple component showing basic parameter binding and rendering
2. **Invoice** - A complex, professional invoice with complete layout including:
   - Header with logo and invoice title
   - Client billing information
   - Payment method and invoice details
   - Items table with descriptions, prices, and totals
   - Financial summary with subtotal, tax, discount, and grand total
   - Terms & conditions
   - Signature block
   - Footer with contact information

To run the samples:

```bash
cd samples/RazorPdf.Sample
dotnet run
```

This will generate two PDF files:
- `sample-output.pdf` - Simple HelloWorld example
- `invoice-sample.pdf` - Complex invoice example

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
