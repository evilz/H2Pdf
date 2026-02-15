# RazorPdf - GitHub Copilot Instructions

## Project Overview

RazorPdf is a .NET library that enables PDF generation using Razor components and C#. It bridges modern .NET UI development with deterministic PDF generation, allowing developers to build PDFs with familiar `.razor` components, strongly typed C#, and dependency injection.

## Technology Stack

- **.NET 10** SDK
- **C# 13** (latest language features)
- **Razor Components** for templating
- **MigraDoc** for PDF document model
- **PdfSharp** for PDF rendering
- **xUnit** for testing
- **BenchmarkDotNet** for performance testing

## Project Structure

```
/src/RazorPdf/          - Core library (main NuGet package)
/tests/RazorPdf.Tests/  - Unit and integration tests
/samples/               - Sample applications demonstrating usage
  /RazorPdf.Sample/     - Main sample with Razor component examples
  /PlaywrightPdf/       - Invoice generation sample
/benchmarks/            - Performance benchmarks
/examples/              - Additional usage examples
```

## Essential Commands

### Build and Test
```bash
# Restore dependencies
dotnet restore RazorPdf.slnx

# Build the solution
dotnet build RazorPdf.slnx --configuration Release

# Run tests
dotnet test RazorPdf.slnx --configuration Release

# Run benchmarks
dotnet run -c Release --project benchmarks/RazorPdf.Benchmarks
```

### Sample Execution
```bash
# Run main sample
cd samples/RazorPdf.Sample
dotnet run

# Run PlaywrightPdf sample
cd samples/PlaywrightPdf
dotnet run
```

## Coding Standards

### General Guidelines

- Follow standard C# naming conventions (PascalCase for public members, camelCase for private fields with underscore prefix)
- Use nullable reference types (`#nullable enable` is default)
- Prefer `var` for local variables when type is obvious
- Use expression-bodied members for simple properties and methods
- Target .NET 10 specifically - use modern C# features when appropriate

### Architecture Patterns

- **Component-to-Document Pipeline**: Razor component → PdfDocumentBuilder → PdfDocumentModel → PDF output
- **Dependency Injection**: Use constructor injection for all services
- **Builder Pattern**: Use fluent APIs for document construction (e.g., `PdfDocumentBuilder`)
- **Separation of Concerns**: Keep rendering logic separate from model construction

### Key Components (Do Not Modify Without Careful Review)

- `PdfRenderer` - High-level API for rendering components to PDF
- `PdfBuildContext` / `PdfBuildContextAccessor` - State management for build pipeline
- `PdfDocumentBuilder` - Fluent document construction API
- `PdfDocumentModelRenderer` - Core renderer from model to MigraDoc/PdfSharp
- `HtmlPdfRenderer` - HTML parsing and rendering pipeline

### Testing Requirements

- Write tests for all new public APIs
- Use xUnit test framework
- Follow existing test patterns in `tests/RazorPdf.Tests/`
- Test names should clearly describe what is being tested (e.g., `Method_Scenario_ExpectedBehavior`)
- Include both unit tests and integration tests where applicable

### Documentation

- Add XML documentation comments for all public APIs
- Update README.md if adding new features or changing usage patterns
- Update Architecture.md if changing core architecture
- Include code examples in XML comments where helpful

## What NOT to Do

### Security and Safety

- **Never commit secrets** or API keys to the repository
- **Never modify .gitignore** to include sensitive files
- **Do not expose internal implementation details** in public APIs
- **Do not introduce breaking changes** without explicit approval

### Build and Dependencies

- **Do not update .NET version** without approval (currently .NET 10)
- **Do not add new NuGet dependencies** without discussion and justification
- **Do not modify the solution file** (.slnx) unless adding/removing projects
- **Do not change benchmark configurations** without understanding performance implications

### Code Quality

- **Do not remove or disable tests** without strong justification
- **Do not skip running tests** before submitting changes
- **Do not add TODO comments** - create issues instead
- **Do not use reflection** when type-safe alternatives exist
- **Avoid platform-specific code** - maintain cross-platform compatibility (Windows, Linux, macOS)

### Areas Requiring Extra Caution

- HTML/CSS parsing logic in `Parsing/` namespace
- MigraDoc rendering pipeline
- Razor component lifecycle management
- Thread safety in rendering pipelines

## Pull Request Guidelines

Before submitting a PR, ensure:

1. Code builds without warnings: `dotnet build RazorPdf.slnx`
2. All tests pass: `dotnet test RazorPdf.slnx`
3. New features include tests
4. Public APIs have XML documentation
5. Changes are described in PR description
6. Breaking changes are clearly marked and justified

## Additional Resources

- [Architecture Documentation](../Architecture.md)
- [Contributing Guidelines](../CONTRIBUTING.md)
- [Roadmap](../ROADMAP.md)

## Workflow Integration

This repository uses GitHub Actions CI/CD:
- CI runs on push to `main` and on all pull requests
- Workflow: restore → build → test → pack
- All checks must pass before merging

## Performance Considerations

- RazorPdf emphasizes deterministic, predictable PDF generation
- Benchmark changes that affect rendering performance
- Be mindful of memory allocations in hot paths
- MigraDoc pipeline trades memory for speed vs. Playwright (see benchmarks)

## Support and Community

- Use GitHub Issues for bugs and feature requests
- Follow existing issue templates
- Be respectful and constructive in discussions
