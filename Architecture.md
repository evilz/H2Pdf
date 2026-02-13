# RazorPdf Architecture

RazorPdf is organized around a component-to-document pipeline.

## Core flow

1. Razor component is instantiated and receives parameters.
2. Component writes content into `PdfDocumentBuilder` via `PdfBuildContext`.
3. Builder produces a `PdfDocumentModel`.
4. Model is rendered by `PdfDocumentModelRenderer` into MigraDoc/PdfSharp output.

## Main components

- `PdfRenderer`: high-level API for rendering components to PDF documents.
- `PdfBuildContext` / `PdfBuildContextAccessor`: state and access for active build pipeline.
- `PdfDocumentBuilder`: fluent document construction primitives.
- `PdfDocumentModelRenderer`: deterministic renderer from model to MigraDoc.
- `HtmlPdfRenderer`: optional HTML parsing/rendering path.

## Parsing subsystem

The `Parsing/` namespace contains HTML/CSS traversal and style resolution primitives used by the HTML pipeline.

## Samples and tests

- `samples/RazorPdf.Sample`: Razor component examples.
- `samples/PlaywrightPdf`: additional invoice-generation sample.
- `tests/RazorPdf.Tests`: unit/integration coverage for rendering behavior.
