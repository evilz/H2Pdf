using System;
using System.Globalization;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using PdfSharpCore.Utils;
using SixLabors.ImageSharp.PixelFormats;

namespace RazorPdf;

/// <summary>
/// Converts <see cref="PdfDocumentModel"/> into a MigraDoc <see cref="Document"/>,
/// applying styles, borders, images, dividers, and layout tables.
/// </summary>
public static class PdfDocumentModelRenderer
{
    private static bool _imageSourceInitialized;
    private static readonly object _lock = new();

    public static Document BuildDocument(PdfDocumentModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        EnsureImageSource();

        var document = new Document();

        // Apply document-level defaults.
        if (!string.IsNullOrEmpty(model.DefaultFontName))
            document.Styles[StyleNames.Normal]!.Font.Name = model.DefaultFontName;
        if (model.DefaultFontSize.HasValue)
            document.Styles[StyleNames.Normal]!.Font.Size = Unit.FromPoint(model.DefaultFontSize.Value);

        if (model.Sections.Count == 0)
        {
            document.AddSection();
            return document;
        }

        foreach (var sectionModel in model.Sections)
        {
            var section = document.AddSection();

            // Apply page margin.
            if (model.PageMarginPt.HasValue)
            {
                var margin = Unit.FromPoint(model.PageMarginPt.Value);
                section.PageSetup.LeftMargin = margin;
                section.PageSetup.RightMargin = margin;
                section.PageSetup.TopMargin = margin;
                section.PageSetup.BottomMargin = margin;
            }

            sectionModel.ConfigurePageSetup?.Invoke(section.PageSetup);

            foreach (var block in sectionModel.Blocks)
                RenderBlock(section, block);
        }

        return document;
    }

    // ══════════════════════════ Block Rendering ═══════════════════════════

    private static void RenderBlock(Section section, PdfBlockModel block)
    {
        switch (block)
        {
            case PdfHeadingModel heading:
                RenderHeading(section, heading);
                break;
            case PdfParagraphModel paragraphModel:
                RenderParagraph(section, paragraphModel);
                break;
            case PdfTableModel tableModel:
                RenderTable(section, tableModel);
                break;
            case PdfDividerModel divider:
                RenderDivider(section, divider);
                break;
            case PdfImageModel image:
                RenderBlockImage(section, image);
                break;
        }
    }

    // ══════════════════════════ Headings ══════════════════════════════════

    private static void RenderHeading(Section section, PdfHeadingModel heading)
    {
        var paragraph = section.AddParagraph();
        paragraph.Style = $"Heading{heading.Level}";
        paragraph.AddText(heading.Text);

        if (heading.Style != null)
            ApplyParagraphStyle(paragraph, heading.Style);
    }

    // ══════════════════════════ Paragraphs ════════════════════════════════

    private static void RenderParagraph(Section section, PdfParagraphModel model)
    {
        var paragraph = section.AddParagraph();
        if (model.Style != null)
            ApplyParagraphStyle(paragraph, model.Style);
        AddParagraphContent(paragraph, model);
    }

    private static Paragraph RenderParagraphInCell(Cell cell, PdfParagraphModel model)
    {
        var paragraph = cell.AddParagraph();
        if (model.Style != null)
            ApplyParagraphStyle(paragraph, model.Style);
        AddParagraphContent(paragraph, model);
        return paragraph;
    }

    private static void AddParagraphContent(Paragraph paragraph, PdfParagraphModel model)
    {
        foreach (var inline in model.Inlines)
        {
            switch (inline)
            {
                case PdfTextRunModel run:
                    if (run.Style is null || !HasStyle(run.Style))
                    {
                        paragraph.AddText(run.Text);
                    }
                    else
                    {
                        var formatted = paragraph.AddFormattedText(run.Text);
                        ApplyTextStyle(formatted.Font, run.Style);
                    }
                    break;
                case PdfLineBreakModel:
                    paragraph.AddLineBreak();
                    break;
                case PdfInlineImageModel imageInline:
                    RenderInlineImage(paragraph, imageInline);
                    break;
            }
        }
    }

    // ══════════════════════════ Dividers ══════════════════════════════════

    private static void RenderDivider(Section section, PdfDividerModel divider)
    {
        // Space before the line.
        if (divider.SpaceBefore > 0)
        {
            var spacer = section.AddParagraph();
            spacer.Format.SpaceAfter = Unit.FromPoint(divider.SpaceBefore);
            spacer.Format.Font.Size = Unit.FromPoint(1);
        }

        // Render a thin colored table row as a horizontal line.
        var table = new Table();
        table.Borders.Width = 0;
        table.AddColumn(Unit.FromCentimeter(18.88));
        var row = table.AddRow();
        var thickness = Math.Max(divider.Thickness, 1.0);
        row.Height = Unit.FromPoint(thickness);
        row.Shading.Color = ParseMigraDocColor(divider.Color) ?? Colors.Gray;
        row.Cells[0].AddParagraph().Format.Font.Size = Unit.FromPoint(1);
        section.Elements.Add(table);

        // Space after the line.
        if (divider.SpaceAfter > 0)
        {
            var spacer = section.AddParagraph();
            spacer.Format.SpaceBefore = Unit.FromPoint(divider.SpaceAfter);
            spacer.Format.Font.Size = Unit.FromPoint(1);
        }
    }

    // ══════════════════════════ Images ════════════════════════════════════

    private static void RenderBlockImage(Section section, PdfImageModel imageModel)
    {
        if (string.IsNullOrEmpty(imageModel.Source) || !File.Exists(imageModel.Source))
            return;

        var paragraph = section.AddParagraph();
        if (imageModel.Alignment.HasValue)
            paragraph.Format.Alignment = ToParagraphAlignment(imageModel.Alignment.Value);

        AddImageToParagraph(paragraph, imageModel.Source, imageModel.WidthPt, imageModel.HeightPt);
    }

    private static void RenderInlineImage(Paragraph paragraph, PdfInlineImageModel imageInline)
    {
        if (string.IsNullOrEmpty(imageInline.Source) || !File.Exists(imageInline.Source))
            return;

        AddImageToParagraph(paragraph, imageInline.Source, imageInline.WidthPt, imageInline.HeightPt);
    }

    private static void AddImageToParagraph(Paragraph paragraph, string source, double? widthPt, double? heightPt)
    {
        try
        {
            var imageKey = ImageSource.FromFile(source);
            var image = paragraph.AddImage(imageKey);
            if (widthPt.HasValue)
                image.Width = Unit.FromPoint(widthPt.Value);
            if (heightPt.HasValue)
                image.Height = Unit.FromPoint(heightPt.Value);
            image.LockAspectRatio = true;
        }
        catch
        {
            // If image loading fails, skip silently.
        }
    }

    // ══════════════════════════ Tables ════════════════════════════════════

    private static void RenderTable(Section section, PdfTableModel tableModel)
    {
        var table = new Table();
        var columnCount = DetermineColumnCount(tableModel);
        if (columnCount == 0) return;

        // Column widths.
        if (tableModel.ColumnWidthsCm is { Count: > 0 })
        {
            for (var i = 0; i < columnCount; i++)
            {
                var widthCm = i < tableModel.ColumnWidthsCm.Count
                    ? tableModel.ColumnWidthsCm[i]
                    : tableModel.ColumnWidthsCm[^1];
                table.AddColumn(Unit.FromCentimeter(widthCm));
            }
        }
        else
        {
            var columnWidth = Unit.FromCentimeter(18.88 / columnCount);
            for (var i = 0; i < columnCount; i++)
                table.AddColumn(columnWidth);
        }

        // Table-level formatting.
        if (!tableModel.IsLayoutTable && tableModel.Borders != null)
        {
            table.Borders.Width = Unit.FromPoint(tableModel.Borders.Width);
            table.Borders.Color = ParseMigraDocColor(tableModel.Borders.Color) ?? Colors.LightGray;
        }
        else if (tableModel.IsLayoutTable)
        {
            table.Borders.Width = 0;
        }

        // Left indent (for right-aligned blocks like totals).
        if (tableModel.LeftIndentCm.HasValue)
        {
            table.Rows.LeftIndent = Unit.FromCentimeter(tableModel.LeftIndentCm.Value);
        }

        // Rows.
        foreach (var rowModel in tableModel.Rows)
        {
            var row = table.AddRow();
            if (rowModel.IsHeader)
            {
                row.HeadingFormat = true;
                row.Format.Font.Bold = true;
            }

            // Background color.
            if (rowModel.BackgroundColor != null)
            {
                var bgColor = ParseMigraDocColor(rowModel.BackgroundColor);
                if (bgColor.HasValue)
                    row.Shading.Color = bgColor.Value;
            }

            for (var i = 0; i < columnCount; i++)
            {
                if (i >= rowModel.Cells.Count) continue;
                var cellModel = rowModel.Cells[i];
                var cell = row.Cells[i];

                // Cell padding.
                if (cellModel.PaddingPt.HasValue)
                {
                    var pad = Unit.FromPoint(cellModel.PaddingPt.Value);
                    cell.Format.LeftIndent = pad;
                }

                // Cell alignment.
                if (cellModel.Alignment.HasValue)
                    cell.Format.Alignment = ToParagraphAlignment(cellModel.Alignment.Value);

                // Render cell content from Blocks.
                var hasBlocks = cellModel.Blocks.Count > 0;
                var hasParagraphs = cellModel.Paragraphs.Count > 0;

                if (hasBlocks)
                {
                    foreach (var block in cellModel.Blocks)
                    {
                        switch (block)
                        {
                            case PdfParagraphModel pm:
                                RenderParagraphInCell(cell, pm);
                                break;
                            case PdfHeadingModel hm:
                                // Headings in cells rendered as bold paragraphs.
                                var hp = cell.AddParagraph();
                                hp.Format.Font.Bold = true;
                                hp.Format.Font.Size = HeadingFontSize(hm.Level);
                                hp.AddText(hm.Text);
                                if (hm.Style != null)
                                    ApplyParagraphStyle(hp, hm.Style);
                                break;
                            case PdfTableModel nestedTable:
                                // Nested table in cell (e.g., flex layout inside flex child).
                                // MigraDoc doesn't directly support nested tables in cells,
                                // so we render the content as paragraphs.
                                RenderNestedTableAsText(cell, nestedTable);
                                break;
                            case PdfDividerModel divider:
                                var dp = cell.AddParagraph();
                                dp.Format.Borders.Bottom.Width = Unit.FromPoint(divider.Thickness);
                                dp.Format.Borders.Bottom.Color = ParseMigraDocColor(divider.Color) ?? Colors.Gray;
                                dp.Format.SpaceBefore = Unit.FromPoint(divider.SpaceBefore);
                                dp.Format.SpaceAfter = Unit.FromPoint(divider.SpaceAfter);
                                break;
                            case PdfImageModel imageModel:
                                if (!string.IsNullOrEmpty(imageModel.Source) && File.Exists(imageModel.Source))
                                {
                                    var imgP = cell.AddParagraph();
                                    AddImageToParagraph(imgP, imageModel.Source, imageModel.WidthPt, imageModel.HeightPt);
                                }
                                break;
                        }
                    }
                }
                else if (hasParagraphs)
                {
                    // Backward compatibility: use Paragraphs list.
                    foreach (var paragraphModel in cellModel.Paragraphs)
                    {
                        RenderParagraphInCell(cell, paragraphModel);
                    }
                }
            }
        }

        // Space before the table.
        if (tableModel.SpaceBeforePt.HasValue)
        {
            var spacer = section.AddParagraph();
            spacer.Format.SpaceAfter = Unit.FromPoint(tableModel.SpaceBeforePt.Value);
            spacer.Format.Font.Size = Unit.FromPoint(1);
        }

        section.Elements.Add(table);
    }

    private static void RenderNestedTableAsText(Cell parentCell, PdfTableModel nestedTable)
    {
        foreach (var row in nestedTable.Rows)
        {
            foreach (var cell in row.Cells)
            {
                foreach (var block in cell.Blocks)
                {
                    if (block is PdfParagraphModel pm)
                        RenderParagraphInCell(parentCell, pm);
                }
                // Also check Paragraphs for backward compat.
                foreach (var pm in cell.Paragraphs)
                    RenderParagraphInCell(parentCell, pm);
            }
        }
    }

    private static int DetermineColumnCount(PdfTableModel tableModel)
    {
        int max = 0;
        foreach (var row in tableModel.Rows)
            if (row.Cells.Count > max)
                max = row.Cells.Count;
        return max;
    }

    // ══════════════════════════ Style Application ═════════════════════════

    private static void ApplyParagraphStyle(Paragraph paragraph, PdfParagraphStyle style)
    {
        if (style.Alignment.HasValue)
            paragraph.Format.Alignment = ToParagraphAlignment(style.Alignment.Value);
        if (style.SpaceBefore.HasValue)
            paragraph.Format.SpaceBefore = Unit.FromPoint(style.SpaceBefore.Value);
        if (style.SpaceAfter.HasValue)
            paragraph.Format.SpaceAfter = Unit.FromPoint(style.SpaceAfter.Value);
        if (!string.IsNullOrEmpty(style.FontName))
            paragraph.Format.Font.Name = style.FontName;
        if (style.FontSize.HasValue)
            paragraph.Format.Font.Size = Unit.FromPoint(style.FontSize.Value);
        if (style.Bold == true)
            paragraph.Format.Font.Bold = true;
        if (style.Italic == true)
            paragraph.Format.Font.Italic = true;
        if (style.FontColor != null)
        {
            var c = ParseMigraDocColor(style.FontColor);
            if (c.HasValue) paragraph.Format.Font.Color = c.Value;
        }
        if (style.LeftIndent.HasValue)
            paragraph.Format.LeftIndent = Unit.FromPoint(style.LeftIndent.Value);
    }

    private static bool HasStyle(PdfTextStyle style)
    {
        return style.Bold.HasValue
            || style.Italic.HasValue
            || style.Underline.HasValue
            || style.FontName != null
            || style.FontSize.HasValue
            || style.Color != null;
    }

    private static void ApplyTextStyle(Font font, PdfTextStyle style)
    {
        if (style.Bold.HasValue)
            font.Bold = style.Bold.Value;
        if (style.Italic.HasValue)
            font.Italic = style.Italic.Value;
        if (style.Underline.HasValue)
            font.Underline = style.Underline.Value ? Underline.Single : Underline.None;
        if (!string.IsNullOrWhiteSpace(style.FontName))
            font.Name = style.FontName;
        if (style.FontSize.HasValue)
            font.Size = Unit.FromPoint(style.FontSize.Value);
        if (style.Color != null)
        {
            var c = ParseMigraDocColor(style.Color);
            if (c.HasValue) font.Color = c.Value;
        }
    }

    // ══════════════════════════ Utilities ═════════════════════════════════

    private static Unit HeadingFontSize(int level) => level switch
    {
        1 => Unit.FromPoint(24),
        2 => Unit.FromPoint(18),
        3 => Unit.FromPoint(14),
        4 => Unit.FromPoint(12),
        5 => Unit.FromPoint(10),
        _ => Unit.FromPoint(9),
    };

    private static ParagraphAlignment ToParagraphAlignment(PdfAlignment alignment) => alignment switch
    {
        PdfAlignment.Left => ParagraphAlignment.Left,
        PdfAlignment.Right => ParagraphAlignment.Right,
        PdfAlignment.Center => ParagraphAlignment.Center,
        PdfAlignment.Justify => ParagraphAlignment.Justify,
        _ => ParagraphAlignment.Left,
    };

    private static Color? ParseMigraDocColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        hex = hex.Trim();

        // Expand shorthand #RGB → #RRGGBB
        if (hex.Length == 4 && hex[0] == '#')
            hex = $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";

        if (hex.Length == 7 && hex[0] == '#')
        {
            if (byte.TryParse(hex[1..3], NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex[3..5], NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex[5..7], NumberStyles.HexNumber, null, out var b))
            {
                return new Color(r, g, b);
            }
        }

        return null;
    }

    private static void EnsureImageSource()
    {
        if (_imageSourceInitialized) return;
        lock (_lock)
        {
            if (_imageSourceInitialized) return;
            try
            {
                if (ImageSource.ImageSourceImpl == null)
                    ImageSource.ImageSourceImpl = new ImageSharpImageSource<Rgba32>();
            }
            catch
            {
                // Image support unavailable; images will be skipped.
            }
            _imageSourceInitialized = true;
        }
    }
}
