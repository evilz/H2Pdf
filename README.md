# RazorPdf

> 🚀 Generate production-ready PDFs using Razor components and C# in minutes.

[![CI](https://github.com/evilz/RazorPdf/actions/workflows/ci.yml/badge.svg)](https://github.com/evilz/RazorPdf/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/RazorPdf.svg)](https://www.nuget.org/packages/RazorPdf)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/evilz/RazorPdf?style=social)](https://github.com/evilz/RazorPdf/stargazers)
[![NuGet downloads](https://img.shields.io/nuget/dt/RazorPdf.svg)](https://www.nuget.org/packages/RazorPdf)


RazorPdf bridges modern .NET UI development and deterministic PDF generation. Build PDFs with familiar `.razor` components, strongly typed C#, and dependency injection.

## ✨ Features

- Razor components as PDF templates
- Fluent PDF document model API
- Optional HTML-to-PDF pipeline (`HtmlPdfRenderer`)
- ASP.NET Core dependency injection integration
- Cross-platform rendering (Windows, Linux, macOS)

## 🚀 Quick Start (30 seconds)

**Requirements:** .NET 10 SDK

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

## 📦 Installation

Install via NuGet:

```bash
dotnet add package RazorPdf
```

Or clone from source:

```bash
git clone https://github.com/evilz/RazorPdf.git
cd RazorPdf
dotnet build
```

## 📖 Usage

```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddRazorPdf();

var provider = services.BuildServiceProvider();
var renderer = provider.GetRequiredService<PdfRenderer>();

var parameters = new Dictionary<string, object?>
{
    ["Name"] = "Developer",
    ["Message"] = "This PDF was generated from a Razor component."
};

var document = await renderer.RenderToPdfAsync<HelloWorld>(parameters);
renderer.SaveToPdf(document, "output.pdf");
```

See [`samples/`](samples/) and [`examples/`](examples/) for complete usage patterns.

## ⚡ Benchmark

Compare Playwright HTML-to-PDF with RazorPdf HTML-to-MigraDoc rendering:

```bash
dotnet run -c Release --project benchmarks/RazorPdf.Benchmarks
```

## 🧠 Why this exists

Most PDF generation tools force teams to use low-level primitives or separate template systems. RazorPdf keeps PDF authoring in your existing .NET workflow with:

- component-driven composition
- familiar Razor syntax
- predictable, code-reviewable output

## 🛣 Roadmap

See [ROADMAP.md](ROADMAP.md).

## 🤝 Contributing

We welcome PRs and ideas. Start with [CONTRIBUTING.md](CONTRIBUTING.md).

## ⭐ Star History

[![Star History Chart](https://api.star-history.com/svg?repos=evilz/RazorPdf&type=Date)](https://star-history.com/#evilz/RazorPdf&Date)

## 🏗 Architecture

High-level internals are documented in [Architecture.md](Architecture.md).

## 📄 License

MIT — see [LICENSE](LICENSE).
