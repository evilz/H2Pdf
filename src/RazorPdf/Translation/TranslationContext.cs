using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using RazorPdf.PdfVdom;

namespace RazorPdf.Translation;

/// <summary>
/// Context passed to translators during VDOM to MigraDoc translation
/// </summary>
public class TranslationContext
{
    public Document Document { get; }
    public PdfRenderOptions Options { get; }

    public Section? CurrentSection { get; set; }
    public Paragraph? CurrentParagraph { get; set; }

    // Table stack for nested tables
    private readonly Stack<Table> _tableStack = new();
    private readonly Stack<Row> _rowStack = new();
    private readonly Stack<Cell> _cellStack = new();

    public Table? CurrentTable => _tableStack.Count > 0 ? _tableStack.Peek() : null;
    public Row? CurrentRow => _rowStack.Count > 0 ? _rowStack.Peek() : null;
    public Cell? CurrentCell => _cellStack.Count > 0 ? _cellStack.Peek() : null;

    public void PushTable(Table table) => _tableStack.Push(table);
    public Table PopTable() => _tableStack.Pop();
    public void PushRow(Row row) => _rowStack.Push(row);
    public Row PopRow() => _rowStack.Pop();
    public void PushCell(Cell cell) => _cellStack.Push(cell);
    public Cell PopCell() => _cellStack.Pop();

    // Style state
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
    public bool IsStrikethrough { get; set; }

    // List tracking
    private readonly Stack<int> _listCounterStack = new();
    private readonly Stack<bool> _listOrderedStack = new();

    public bool InList => _listCounterStack.Count > 0;
    public bool IsOrderedList => _listOrderedStack.Count > 0 && _listOrderedStack.Peek();

    public void PushList(bool ordered)
    {
        _listOrderedStack.Push(ordered);
        _listCounterStack.Push(0);
    }

    public void PopList()
    {
        _listOrderedStack.Pop();
        _listCounterStack.Pop();
    }

    public int IncrementListCounter()
    {
        var current = _listCounterStack.Pop();
        current++;
        _listCounterStack.Push(current);
        return current;
    }

    // Column count tracking for tables
    public int TableColumnCount { get; set; }

    // Translator registry for recursive translation
    internal Action<VNode, TranslationContext>? TranslateChild { get; set; }

    public TranslationContext(Document document, PdfRenderOptions? options = null)
    {
        Document = document;
        Options = options ?? new PdfRenderOptions();
    }

    /// <summary>
    /// Ensures a section exists, creating one if needed
    /// </summary>
    public Section EnsureSection()
    {
        if (CurrentSection == null)
        {
            CurrentSection = Document.AddSection();
            ApplyPageSetup(CurrentSection);
        }
        return CurrentSection;
    }

    /// <summary>
    /// Ensures a paragraph exists in the current context (section or cell)
    /// </summary>
    public Paragraph EnsureParagraph()
    {
        if (CurrentParagraph == null)
        {
            if (CurrentCell != null)
            {
                CurrentParagraph = CurrentCell.AddParagraph();
            }
            else
            {
                var section = EnsureSection();
                CurrentParagraph = section.AddParagraph();
            }
        }
        return CurrentParagraph;
    }

    /// <summary>
    /// Adds text with current formatting to the current paragraph
    /// </summary>
    public void AddFormattedText(string text)
    {
        var paragraph = EnsureParagraph();

        if (!IsBold && !IsItalic && !IsUnderline && !IsStrikethrough)
        {
            paragraph.AddText(text);
            return;
        }

        var format = paragraph.AddFormattedText(text);

        if (IsBold)
            format.Bold = true;
        if (IsItalic)
            format.Italic = true;
        if (IsUnderline)
            format.Underline = Underline.Single;
        // MigraDoc does not natively support strikethrough; style flag is tracked for extensibility
    }

    private void ApplyPageSetup(Section section)
    {
        section.PageSetup.PageFormat = Options.PageSize;
        section.PageSetup.Orientation = Options.PageOrientation;
        section.PageSetup.TopMargin = Unit.FromCentimeter(Options.MarginTop);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(Options.MarginBottom);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(Options.MarginLeft);
        section.PageSetup.RightMargin = Unit.FromCentimeter(Options.MarginRight);
    }
}
