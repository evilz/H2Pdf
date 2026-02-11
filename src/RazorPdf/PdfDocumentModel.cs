using System;
using System.Collections.Generic;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf;

/// <summary>
/// Model representing a PDF document structure emitted by Razor components.
/// </summary>
public sealed class PdfDocumentModel
{
    public IList<PdfSectionModel> Sections { get; } = new List<PdfSectionModel>();
}

public sealed class PdfSectionModel
{
    public IList<PdfBlockModel> Blocks { get; } = new List<PdfBlockModel>();
    public Action<PageSetup>? ConfigurePageSetup { get; set; }
}

public abstract class PdfBlockModel
{
}

public sealed class PdfHeadingModel : PdfBlockModel
{
    public PdfHeadingModel(string text, int level)
    {
        Text = text;
        Level = level;
    }

    public string Text { get; }
    public int Level { get; }
}

public sealed class PdfParagraphModel : PdfBlockModel
{
    public PdfParagraphModel()
    {
    }

    public PdfParagraphModel(string text)
    {
        AddText(text);
    }

    public IList<PdfInlineModel> Inlines { get; } = new List<PdfInlineModel>();

    public PdfParagraphModel AddText(string text, PdfTextStyle? style = null)
    {
        Inlines.Add(new PdfTextRunModel(text, style));
        return this;
    }

    public PdfParagraphModel AddLineBreak()
    {
        Inlines.Add(new PdfLineBreakModel());
        return this;
    }
}

public abstract class PdfInlineModel
{
}

public sealed class PdfTextRunModel : PdfInlineModel
{
    public PdfTextRunModel(string text, PdfTextStyle? style = null)
    {
        Text = text;
        Style = style;
    }

    public string Text { get; }
    public PdfTextStyle? Style { get; }
}

public sealed class PdfLineBreakModel : PdfInlineModel
{
}

public sealed class PdfTextStyle
{
    public bool? Bold { get; set; }
    public bool? Italic { get; set; }
    public bool? Underline { get; set; }
    public string? FontName { get; set; }
    public double? FontSize { get; set; }
}

public sealed class PdfTableModel : PdfBlockModel
{
    public IList<PdfTableRowModel> Rows { get; } = new List<PdfTableRowModel>();
}

public sealed class PdfTableRowModel
{
    public PdfTableRowModel(bool isHeader = false)
    {
        IsHeader = isHeader;
    }

    public bool IsHeader { get; }
    public IList<PdfTableCellModel> Cells { get; } = new List<PdfTableCellModel>();
}

public sealed class PdfTableCellModel
{
    public IList<PdfParagraphModel> Paragraphs { get; } = new List<PdfParagraphModel>();
}
