using System;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace RazorPdf;

/// <summary>
/// Converts PDF document models into MigraDoc documents.
/// </summary>
public static class PdfDocumentModelRenderer
{
    public static Document BuildDocument(PdfDocumentModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var document = new Document();
        if (model.Sections.Count == 0)
        {
            document.AddSection();
            return document;
        }

        foreach (var sectionModel in model.Sections)
        {
            var section = document.AddSection();
            sectionModel.ConfigurePageSetup?.Invoke(section.PageSetup);

            foreach (var block in sectionModel.Blocks)
            {
                switch (block)
                {
                    case PdfHeadingModel heading:
                        var headingParagraph = section.AddParagraph();
                        headingParagraph.Style = $"Heading{heading.Level}";
                        AddParagraphContent(headingParagraph, new PdfParagraphModel(heading.Text));
                        break;
                    case PdfParagraphModel paragraphModel:
                        var paragraph = section.AddParagraph();
                        AddParagraphContent(paragraph, paragraphModel);
                        break;
                    case PdfTableModel tableModel:
                        section.Elements.Add(BuildTable(tableModel));
                        break;
                }
            }
        }

        return document;
    }

    private static void AddParagraphContent(Paragraph paragraph, PdfParagraphModel model)
    {
        if (model.Inlines.Count == 0)
        {
            return;
        }

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
                        ApplyStyle(formatted.Font, run.Style);
                    }
                    break;
                case PdfLineBreakModel:
                    paragraph.AddLineBreak();
                    break;
            }
        }
    }

    private static bool HasStyle(PdfTextStyle style)
    {
        return style.Bold.HasValue
            || style.Italic.HasValue
            || style.Underline.HasValue
            || style.FontName != null
            || style.FontSize.HasValue;
    }

    private static void ApplyStyle(Font font, PdfTextStyle style)
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
    }

    private static Table BuildTable(PdfTableModel tableModel)
    {
        var table = new Table();
        var columnCount = 0;
        foreach (var row in tableModel.Rows)
        {
            if (row.Cells.Count > columnCount)
                columnCount = row.Cells.Count;
        }

        if (columnCount == 0)
        {
            return table;
        }

        var columnWidth = Unit.FromCentimeter(16.0 / columnCount);
        for (var i = 0; i < columnCount; i++)
        {
            table.AddColumn(columnWidth);
        }

        foreach (var rowModel in tableModel.Rows)
        {
            var row = table.AddRow();
            if (rowModel.IsHeader)
            {
                row.HeadingFormat = true;
                row.Format.Font.Bold = true;
            }

            for (var i = 0; i < columnCount; i++)
            {
                var cell = row.Cells[i];
                if (i >= rowModel.Cells.Count)
                    continue;

                var cellModel = rowModel.Cells[i];
                if (cellModel.Paragraphs.Count == 0)
                    continue;

                foreach (var paragraphModel in cellModel.Paragraphs)
                {
                    var paragraph = cell.AddParagraph();
                    AddParagraphContent(paragraph, paragraphModel);
                }
            }
        }

        return table;
    }
}
