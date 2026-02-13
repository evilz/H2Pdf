using System;
using System.Collections.Generic;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf;

/// <summary>
/// Builder for creating PDF documents with fluent API
/// </summary>
public class PdfDocumentBuilder
{
    private readonly PdfDocumentModel _document;
    private PdfSectionModel? _currentSection;

    public PdfDocumentBuilder()
    {
        _document = new PdfDocumentModel();
    }

    /// <summary>
    /// Adds a new section to the document
    /// </summary>
    public PdfDocumentBuilder AddSection()
    {
        _currentSection = new PdfSectionModel();
        _document.Sections.Add(_currentSection);
        return this;
    }

    /// <summary>
    /// Adds a heading to the current section
    /// </summary>
    /// <param name="text">The heading text</param>
    /// <param name="level">The heading level (1-6)</param>
    public PdfDocumentBuilder AddHeading(string text, int level = 1)
    {
        if (level < 1 || level > 6)
            throw new ArgumentOutOfRangeException(nameof(level), "Heading level must be between 1 and 6.");
        
        EnsureSection();
        _currentSection!.Blocks.Add(new PdfHeadingModel(text, level));
        return this;
    }

    /// <summary>
    /// Adds a paragraph to the current section
    /// </summary>
    public PdfDocumentBuilder AddParagraph(string text)
    {
        EnsureSection();
        _currentSection!.Blocks.Add(new PdfParagraphModel(text));
        return this;
    }

    /// <summary>
    /// Adds a paragraph to the current section with richer formatting
    /// </summary>
    public PdfDocumentBuilder AddParagraph(Action<PdfParagraphBuilder> configure)
    {
        EnsureSection();
        var paragraph = new PdfParagraphModel();
        var builder = new PdfParagraphBuilder(paragraph);
        configure(builder);
        _currentSection!.Blocks.Add(paragraph);
        return this;
    }

    /// <summary>
    /// Adds a table to the current section
    /// </summary>
    public PdfDocumentBuilder AddTable(Action<PdfTableBuilder> configure)
    {
        EnsureSection();
        var table = new PdfTableModel();
        var builder = new PdfTableBuilder(table);
        configure(builder);
        _currentSection!.Blocks.Add(table);
        return this;
    }

    /// <summary>
    /// Sets the page setup for the current section
    /// </summary>
    public PdfDocumentBuilder SetPageSetup(Action<PageSetup> configure)
    {
        EnsureSection();
        _currentSection!.ConfigurePageSetup = configure;
        return this;
    }

    /// <summary>
    /// Builds and returns the document
    /// </summary>
    public PdfDocumentModel Build()
    {
        return _document;
    }

    private void EnsureSection()
    {
        if (_currentSection == null)
        {
            AddSection();
        }
    }
}

/// <summary>
/// Builder for paragraph content
/// </summary>
public sealed class PdfParagraphBuilder
{
    private readonly PdfParagraphModel _paragraph;

    internal PdfParagraphBuilder(PdfParagraphModel paragraph)
    {
        _paragraph = paragraph;
    }

    public PdfParagraphBuilder AddText(string text, PdfTextStyle? style = null)
    {
        _paragraph.Inlines.Add(new PdfTextRunModel(text, style));
        return this;
    }

    public PdfParagraphBuilder AddLineBreak()
    {
        _paragraph.Inlines.Add(new PdfLineBreakModel());
        return this;
    }
}

/// <summary>
/// Builder for table content
/// </summary>
public sealed class PdfTableBuilder
{
    private readonly PdfTableModel _table;

    internal PdfTableBuilder(PdfTableModel table)
    {
        _table = table;
    }

    public PdfTableBuilder AddRow(params string[] cells)
    {
        return AddRow(false, cells);
    }

    public PdfTableBuilder AddHeaderRow(params string[] cells)
    {
        return AddRow(true, cells);
    }

    public PdfTableBuilder AddRow(bool isHeader, IEnumerable<string> cells)
    {
        if (cells == null)
            throw new ArgumentNullException(nameof(cells));

        var row = new PdfTableRowModel(isHeader);
        foreach (var cellText in cells)
        {
            var cell = new PdfTableCellModel();
            cell.Paragraphs.Add(new PdfParagraphModel(cellText));
            cell.Blocks.Add(new PdfParagraphModel(cellText));
            row.Cells.Add(cell);
        }
        _table.Rows.Add(row);
        return this;
    }
}
