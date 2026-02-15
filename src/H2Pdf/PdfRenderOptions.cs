using System;

namespace H2Pdf;

/// <summary>
/// Options for customizing PDF rendering.
/// </summary>
public sealed class PdfRenderOptions
{
    public Action<PdfDocumentBuilder>? ConfigureDocument { get; init; }
}
