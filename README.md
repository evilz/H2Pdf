# RazorPdf

RazorPdf bridges the gap between modern web UI development and PDF generation. It lets you create sophisticated PDFs using Razor components with interactive rich styling and familiar development patterns.

RazorPdf is a .NET framework that enables developers to use ASP.NET Core Razor components (typically used for web applications) to generate PDF documents. It translates the virtual DOM using a renderer to PDF using the MigraDoc Library.

## Features

- **Razor Component Rendering**: Use familiar ASP.NET Core Razor components to define PDF content
- **Type-Safe**: Leverage C# and .NET type system for building PDF documents
- **Fluent API**: Build PDF documents programmatically with an intuitive fluent interface
- **Cross-Platform**: Works on Windows, Linux, and macOS thanks to PdfSharpCore and MigraDocCore
- **Dependency Injection Support**: Integrate seamlessly with ASP.NET Core DI container

## Installation

> **Note:** RazorPdf is not yet published to NuGet. Until it is available, you should build and reference it from source. See [Building from Source](#building-from-source) for instructions.

## Quick Start

### 1. Create a Razor Component

Create a `.razor` file (e.g., `HelloWorld.razor`):

```razor
@using Microsoft.AspNetCore.Components

<div class="greeting">
    <h1>Hello, RazorPdf!</h1>
    <p>Welcome @Name to PDF generation with Razor components!</p>
    
    @if (!string.IsNullOrEmpty(Message))
    {
        <p><strong>@Message</strong></p>
    }
</div>

@code {
    [Parameter]
    public string Name { get; set; } = "World";
    
    [Parameter]
    public string? Message { get; set; }
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
