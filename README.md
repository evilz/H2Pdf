# RazorPdf

RazorPdf bridges the gap between modern web UI development and PDF generation. It lets you create sophisticated PDFs using Razor components with interactive rich styling and familiar development patterns.

RazorPdf is a .NET framework that enables developers to build web applications using ASP.NET Core Razor components and translate them into PDF documents. It translates the virtual DOM using a renderer to PDF using the MigraDoc Library.

## Features

- **Razor Component Rendering**: Use familiar ASP.NET Core Razor components to define PDF content
- **Type-Safe**: Leverage C# and .NET type system for building PDF documents
- **Fluent API**: Build PDF documents programmatically with an intuitive fluent interface
- **Cross-Platform**: Works on Windows, Linux, and macOS thanks to PdfSharpCore and MigraDocCore
- **Dependency Injection Support**: Integrate seamlessly with ASP.NET Core DI container

## Installation

```bash
dotnet add package RazorPdf
```

## Quick Start

### 1. Create a Razor Component

```csharp
public class MyPdfComponent : ComponentBase
{
    [Parameter]
    public string Title { get; set; } = "Hello PDF!";
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.OpenElement(1, "h1");
        builder.AddContent(2, Title);
        builder.CloseElement();
        builder.CloseElement();
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
    { "Title", "My First PDF" }
};

var document = await pdfRenderer.RenderToPdfAsync<MyPdfComponent>(parameters);
pdfRenderer.SaveToPdf(document, "output.pdf");
```

## Building from Source

```bash
git clone https://github.com/evilz/RazorPdf.git
cd RazorPdf
dotnet build
```

## Running the Sample

```bash
cd samples/RazorPdf.Sample
dotnet run
```

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
