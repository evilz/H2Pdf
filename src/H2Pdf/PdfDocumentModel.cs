using System;
using System.Collections.Generic;
using MigraDocCore.DocumentObjectModel;

namespace H2Pdf;

/// <summary>
/// Model representing a PDF document structure emitted by Razor components.
/// </summary>
public sealed class PdfDocumentModel
{
    public List<PdfSectionModel> Sections { get; } = new();

    /// <summary>Default font family for the entire document.</summary>
    public string? DefaultFontName { get; set; }

    /// <summary>Default font size (in points) for the entire document.</summary>
    public double? DefaultFontSize { get; set; }

    /// <summary>Page margin in points (applied to all sides).</summary>
    public double? PageMarginPt { get; set; }

    /// <summary>Preferred content width in centimeters for layout calculations.</summary>
    public double? ContentWidthCm { get; set; }
}

public sealed class PdfSectionModel
{
    public List<PdfBlockModel> Blocks { get; } = new();
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

    /// <summary>Optional style override for the heading.</summary>
    public PdfParagraphStyle? Style { get; set; }
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

    public List<PdfInlineModel> Inlines { get; } = new();

    /// <summary>Paragraph-level style (alignment, spacing, font defaults).</summary>
    public PdfParagraphStyle? Style { get; set; }

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

/// <summary>
/// Paragraph-level formatting (alignment, spacing, default font properties).
/// </summary>
public sealed class PdfParagraphStyle
{
    public PdfAlignment? Alignment { get; set; }
    public double? SpaceBefore { get; set; }
    public double? SpaceAfter { get; set; }
    public string? FontName { get; set; }
    public double? FontSize { get; set; }
    public string? FontColor { get; set; }
    public bool? Bold { get; set; }
    public bool? Italic { get; set; }
    public double? LeftIndent { get; set; }
}

public enum PdfAlignment { Left, Center, Right, Justify }

/// <summary>
/// A horizontal divider line (maps to an HTML element with border-top and no content).
/// </summary>
public sealed class PdfDividerModel : PdfBlockModel
{
    public string Color { get; set; } = "#aaaaaa";
    public double Thickness { get; set; } = 0.5;
    public double SpaceBefore { get; set; } = 10;
    public double SpaceAfter { get; set; } = 10;
}

/// <summary>
/// A block-level image.
/// </summary>
public sealed class PdfImageModel : PdfBlockModel
{
    public string Source { get; set; } = string.Empty;
    public double? WidthPt { get; set; }
    public double? HeightPt { get; set; }
    public PdfAlignment? Alignment { get; set; }
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

/// <summary>
/// An inline image (placed within a paragraph).
/// </summary>
public sealed class PdfInlineImageModel : PdfInlineModel
{
    public string Source { get; set; } = string.Empty;
    public double? WidthPt { get; set; }
    public double? HeightPt { get; set; }
}

public sealed class PdfTextStyle
{
    public bool? Bold { get; set; }
    public bool? Italic { get; set; }
    public bool? Underline { get; set; }
    public string? FontName { get; set; }
    public double? FontSize { get; set; }
    public string? Color { get; set; }
}

/// <summary>Border style for tables.</summary>
public sealed class PdfTableBorderStyle
{
    public double Width { get; set; } = 0.5;
    public string Color { get; set; } = "#dddddd";
}

public sealed class PdfTableModel : PdfBlockModel
{
    public List<PdfTableRowModel> Rows { get; } = new();

    /// <summary>If true, this is a layout-only table (no visible borders).</summary>
    public bool IsLayoutTable { get; set; }

    /// <summary>Column widths in centimeters. If null, columns are auto-sized.</summary>
    public List<double>? ColumnWidthsCm { get; set; }

    /// <summary>Border style for content tables.</summary>
    public PdfTableBorderStyle? Borders { get; set; }

    /// <summary>Space before the table in points.</summary>
    public double? SpaceBeforePt { get; set; }

    /// <summary>Left indent of the table in centimeters (for right-aligned blocks).</summary>
    public double? LeftIndentCm { get; set; }
}

public sealed class PdfTableRowModel
{
    public PdfTableRowModel(bool isHeader = false)
    {
        IsHeader = isHeader;
    }

    public bool IsHeader { get; }
    public List<PdfTableCellModel> Cells { get; } = new();

    /// <summary>Background color (hex, e.g. "#f0f0f0").</summary>
    public string? BackgroundColor { get; set; }
}

public sealed class PdfTableCellModel
{
    /// <summary>General block content (paragraphs, nested tables, images, etc.).</summary>
    public List<PdfBlockModel> Blocks { get; } = new();

    /// <summary>Backward-compatible paragraph list. Use <see cref="Blocks"/> for new code.</summary>
    public List<PdfParagraphModel> Paragraphs { get; } = new();

    /// <summary>Cell padding in points.</summary>
    public double? PaddingPt { get; set; }

    /// <summary>Horizontal alignment of cell content.</summary>
    public PdfAlignment? Alignment { get; set; }
}
