# H2Pdf

H2Pdf supports both .NET 8 LTS and .NET 10. NuGet selects the matching target automatically.

> 🚀 Generate production-ready PDFs using Razor components and C# in minutes.

[![CI](https://github.com/evilz/H2Pdf/actions/workflows/ci.yml/badge.svg)](https://github.com/evilz/H2Pdf/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/H2Pdf.svg)](https://www.nuget.org/packages/H2Pdf)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/evilz/H2Pdf?style=social)](https://github.com/evilz/H2Pdf/stargazers)
[![NuGet downloads](https://img.shields.io/nuget/dt/H2Pdf.svg)](https://www.nuget.org/packages/H2Pdf)


H2Pdf bridges modern .NET UI development and deterministic PDF generation. Build PDFs with familiar `.razor` components, strongly typed C#, and dependency injection.

## ✨ Features

- Razor components as PDF templates
- Fluent PDF document model API
- Optional HTML-to-PDF pipeline (`HtmlPdfRenderer`)
- ASP.NET Core dependency injection integration
- Cross-platform rendering (Windows, Linux, macOS)

## 🚀 Quick Start (30 seconds)

**Requirements:** .NET 10 SDK

```bash
git clone https://github.com/evilz/H2Pdf.git
cd H2Pdf
dotnet build
cd samples/H2Pdf.Sample
dotnet run
```

Generated files:

- `sample-output.pdf`
- `invoice-sample.pdf`

## 📦 Installation

Install via NuGet:

```bash
dotnet add package H2Pdf
```

Or clone from source:

```bash
git clone https://github.com/evilz/H2Pdf.git
cd H2Pdf
dotnet build
```

## 📖 Usage

```csharp
var services = new ServiceCollection();
services.AddLogging();
services.AddH2Pdf();

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

Compare Playwright HTML-to-PDF with H2Pdf HTML-to-MigraDoc rendering:

```bash
dotnet build benchmarks/H2Pdf.Benchmarks
pwsh benchmarks/H2Pdf.Benchmarks/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet run -c Release --project benchmarks/H2Pdf.Benchmarks
```

**Environment**

-   BenchmarkDotNet v0.14.0
-   .NET 10.0.2 (RyuJIT, AVX2)
-   Windows 11 x64
-   GC: Concurrent Workstation


### 🧪 Benchmarked Pipelines

1.  **Playwright (Chromium)**
    -   HTML rendered in headless Chromium
    -   PDF generated via `Page.PdfAsync()`

2.  **MigraDoc (via HtmlPdfRenderer)**
    -   HTML → MigraDoc DOM
    -   MigraDoc → PDF via `PdfDocumentRenderer`


### 📈 Example output (single run)

| Method | Mean | StdDev | Gen0 | Gen1 | Allocated |
| --- | --- | --- | --- | --- | --- |
| PlaywrightHtmlToPdfAsync | 9.231 ms | 0.196 ms | 15.63 | - | 188 KB |
| MigraDoc_HtmlToPdf | 4.684 ms | 0.014 ms | 312.50 | 109.38 | 2.68 MB |

> These numbers are an example from one environment and one HTML payload.  
> Results will vary by machine, OS, runtime version, browser version, and template complexity.

### ⏱ Latency per PDF (example)

-   MigraDoc: ~4.68 ms
-   Playwright: ~9.23 ms

### 📊 Stability (Jitter, example)

| Pipeline | StdDev |
| --- | --- |
| MigraDoc | 0.014 ms |
| Playwright | 0.196 ms |

### 🧠 Memory Behavior (example)

| Pipeline | Allocated per PDF |
| --- | --- |
| Playwright | ~188 KB |
| MigraDoc | ~2.68 MB |

GC activity:

-   MigraDoc → Triggers Gen0 + Gen1
-   Playwright → Mostly Gen0 only

Use benchmark output in your environment to compare trade-offs for your workloads.


## 🧠 Why this exists

Most PDF generation tools force teams to use low-level primitives or separate template systems. H2Pdf keeps PDF authoring in your existing .NET workflow with:

- component-driven composition
- familiar Razor syntax
- predictable, code-reviewable output

## 🛣 Roadmap

See [ROADMAP.md](ROADMAP.md).

## 🤝 Contributing

We welcome PRs and ideas. Start with [CONTRIBUTING.md](CONTRIBUTING.md).

## ⭐ Star History

[![Star History Chart](https://api.star-history.com/svg?repos=evilz/H2Pdf&type=Date)](https://star-history.com/#evilz/H2Pdf&Date)

## 🏗 Architecture

High-level internals are documented in [Architecture.md](Architecture.md).

## 📄 License

MIT — see [LICENSE](LICENSE).
