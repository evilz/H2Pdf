using MigraDoc.DocumentObjectModel;

namespace RazorPdf;

/// <summary>
/// Builder for creating PDF documents with fluent API
/// </summary>
public class PdfDocumentBuilder
{
    private readonly Document _document;
    private Section? _currentSection;

    public PdfDocumentBuilder()
    {
        _document = new Document();
    }

    /// <summary>
    /// Adds a new section to the document
    /// </summary>
    public PdfDocumentBuilder AddSection()
    {
        _currentSection = _document.AddSection();
        return this;
    }

    /// <summary>
    /// Adds a heading to the current section
    /// </summary>
    public PdfDocumentBuilder AddHeading(string text, int level = 1)
    {
        EnsureSection();
        var paragraph = _currentSection!.AddParagraph(text);
        paragraph.Style = $"Heading{level}";
        return this;
    }

    /// <summary>
    /// Adds a paragraph to the current section
    /// </summary>
    public PdfDocumentBuilder AddParagraph(string text)
    {
        EnsureSection();
        _currentSection!.AddParagraph(text);
        return this;
    }

    /// <summary>
    /// Sets the page setup for the current section
    /// </summary>
    public PdfDocumentBuilder SetPageSetup(Action<PageSetup> configure)
    {
        EnsureSection();
        configure(_currentSection!.PageSetup);
        return this;
    }

    /// <summary>
    /// Builds and returns the document
    /// </summary>
    public Document Build()
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
