# RazorPdfKit

> 🚀 Build production-ready PDFs from Razor components and C#.

[![CI](https://github.com/evilz/RazorPdf/actions/workflows/ci.yml/badge.svg)](https://github.com/evilz/RazorPdf/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/RazorPdfKit.svg)](https://www.nuget.org/packages/RazorPdfKit)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/evilz/RazorPdf?style=social)](https://github.com/evilz/RazorPdf/stargazers)
[![NuGet downloads](https://img.shields.io/nuget/dt/RazorPdfKit.svg)](https://www.nuget.org/packages/RazorPdfKit)

RazorPdfKit bridges modern .NET UI development and deterministic PDF generation. Author templates with `.razor` components, inject your services, and export consistent documents from code.

## Why RazorPdfKit?

- **Use Razor as your template engine**: no extra templating language to learn.
- **Stay type-safe**: pass strongly typed models and component parameters.
- **Keep architecture clean**: integrate through DI and familiar ASP.NET Core patterns.
- **Render across platforms**: Windows, Linux, and macOS support.

## Features

- Razor components as PDF templates
- Fluent PDF document model API
- Optional HTML-to-PDF pipeline (`HtmlPdfRenderer`)
- ASP.NET Core dependency injection integration
- Cross-platform rendering (Windows, Linux, macOS)

## Requirements

- .NET 10 SDK

## Installation

```bash
dotnet add package RazorPdfKit
```

## Quick start

```bash
git clone https://github.com/evilz/RazorPdf.git
cd RazorPdf
dotnet build
cd samples/RazorPdf.Sample
dotnet run
```

Generated files:

- `sample-output.pdf`
- `invoice-sample.pdf`

## Basic usage

```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddRazorPdf();

var provider = services.BuildServiceProvider();
var renderer = provider.GetRequiredService<PdfRenderer>();

var parameters = new Dictionary<string, object?>
{
    ["Name"] = "Developer",
    ["Message"] = "Generated with RazorPdfKit."
};

var document = await renderer.RenderToPdfAsync<HelloWorld>(parameters);
renderer.SaveToPdf(document, "output.pdf");
```

## Documentation map

- [examples/README.md](examples/README.md): runnable examples and sample entry points
- [Architecture.md](Architecture.md): component-to-document pipeline internals
- [CONTRIBUTING.md](CONTRIBUTING.md): contribution flow and development setup
- [ROADMAP.md](ROADMAP.md): planned enhancements and milestones

## Project status

The library is actively evolving. Feedback, feature requests, and PRs are welcome.

## Contributing

Start with [CONTRIBUTING.md](CONTRIBUTING.md).

## License

MIT — see [LICENSE](LICENSE).
