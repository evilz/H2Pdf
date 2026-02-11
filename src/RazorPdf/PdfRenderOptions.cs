using System;

namespace RazorPdf;

/// <summary>
/// Options for customizing PDF rendering.
/// </summary>
public sealed class PdfRenderOptions
{
    public Action<PdfDocumentBuilder>? ConfigureDocument { get; init; }
}
