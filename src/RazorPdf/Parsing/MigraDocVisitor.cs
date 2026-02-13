using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace RazorPdf.Parsing;

/// <summary>
/// An <see cref="IHtmlNodeVisitor"/> that converts an AngleSharp HTML DOM tree
/// into a <see cref="PdfDocumentModel"/>, applying CSS styles, handling flex layouts,
/// images, and dividers.
/// </summary>
public sealed partial class MigraDocVisitor : IHtmlNodeVisitor
{
    private readonly PdfDocumentModel _document = new();
    private readonly PdfSectionModel _currentSection;
    private readonly CssStyleResolver _cssResolver;
    private readonly string? _basePath;
    private readonly double _contentWidthCm;

    // Current paragraph being assembled.
    private PdfParagraphModel? _currentParagraph;

    // Heading accumulation state.
    private int _headingLevel;
    private StringBuilder? _headingText;
    private PdfParagraphStyle? _headingStyle;

    // HTML Table state.
    private PdfTableModel? _currentTable;
    private PdfTableRowModel? _currentRow;
    private PdfTableCellModel? _currentCell;
    private bool _isHeaderRow;

    // Inline style stack.
    private readonly Stack<PdfTextStyle> _styleStack = new();

    // Track which elements pushed a style so we can pop correctly.
    private readonly HashSet<int> _cssPushedElements = new();

    // Skip depth (for <head>, <style>, <script>, etc.).
    private int _skipDepth;

    // Element depth for flex container tracking.
    private int _elementDepth;

    // Flex container stack.
    private readonly Stack<FlexContainerInfo> _flexStack = new();

    private static readonly HashSet<string> SkipTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "head", "style", "script", "meta", "link", "title", "noscript"
    };

    public MigraDocVisitor() : this(new CssStyleResolver(), null, 18.88)
    {
    }

    public MigraDocVisitor(CssStyleResolver cssResolver, string? basePath, double contentWidthCm)
    {
        _cssResolver = cssResolver;
        _basePath = basePath;
        _contentWidthCm = contentWidthCm;
        _currentSection = new PdfSectionModel();
        _document.Sections.Add(_currentSection);
    }

    /// <summary>
    /// Returns the <see cref="PdfDocumentModel"/> built from the visited HTML tree.
    /// </summary>
    public PdfDocumentModel GetResult() => _document;

    // ══════════════════════════ IHtmlNodeVisitor ══════════════════════════

    public void EnterElement(IElement element)
    {
        var tag = element.TagName.ToLowerInvariant();

        // Skip subtrees.
        if (_skipDepth > 0 || SkipTags.Contains(tag))
        {
            _skipDepth++;
            return;
        }

        _elementDepth++;
        // Resolve CSS for this element.
        var css = _cssResolver.ResolveWithInheritance(element);
        var display = css.GetValueOrDefault("display") ?? CssValueParser.GetDefaultDisplay(tag);

        // ── Check if we are entering a direct child of a flex container ──
        if (_flexStack.Count > 0)
        {
            var parentFlex = _flexStack.Peek();
            if (_elementDepth == parentFlex.Depth + 1)
            {
                // Start a new cell for this flex child.
                FlushParagraph();
                var cell = new PdfTableCellModel();
                var align = CssValueParser.ParseAlignment(css.GetValueOrDefault("text-align"));
                if (align.HasValue) cell.Alignment = align;
                _currentCell = cell;
                _currentParagraph = null;
            }
        }

        // ── Check if THIS element is a flex container ──
        if (display.Contains("flex"))
        {
            FlushParagraph();
            EnterFlexContainer(element, css);
            return;
        }

        // ── Divider detection (empty block with border-top) ──
        if (display == "block" && string.IsNullOrWhiteSpace(element.TextContent))
        {
            var borderTop = css.GetValueOrDefault("border-top");
            if (!string.IsNullOrEmpty(borderTop) && borderTop != "none")
            {
                FlushParagraph();
                var divider = new PdfDividerModel();
                var border = CssValueParser.ParseBorder(borderTop);
                if (border.HasValue)
                {
                    divider.Thickness = border.Value.ThicknessPt;
                    divider.Color = border.Value.Color ?? "#aaaaaa";
                }
                var marginStr = css.GetValueOrDefault("margin");
                if (marginStr != null)
                {
                    var m = CssValueParser.ParseMarginShorthand(marginStr);
                    divider.SpaceBefore = m.Top;
                    divider.SpaceAfter = m.Bottom;
                }
                AddBlockToTarget(divider);
                _skipDepth = 1;
                return;
            }
        }

        switch (tag)
        {
            // ── Headings ─────────────────────────────────────────────────
            case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                FlushParagraph();
                _headingLevel = tag[1] - '0';
                _headingText = new StringBuilder();
                _headingStyle = BuildParagraphStyle(css);
                break;

            // ── HTML Tables ──────────────────────────────────────────────
            case "table":
                FlushParagraph();
                _currentTable = new PdfTableModel
                {
                    Borders = new PdfTableBorderStyle { Width = 0.5, Color = "#dddddd" }
                };
                var mtStr = css.GetValueOrDefault("margin-top");
                if (mtStr != null)
                    _currentTable.SpaceBeforePt = CssValueParser.ParseLength(mtStr);
                break;
            case "thead":
                _isHeaderRow = true;
                break;
            case "tbody":
            case "tfoot":
                _isHeaderRow = false;
                break;
            case "tr":
                _currentRow = new PdfTableRowModel(_isHeaderRow);
                if (_isHeaderRow)
                    _currentRow.BackgroundColor = "#f0f0f0";
                break;
            case "th":
                _currentCell = new PdfTableCellModel { PaddingPt = 7.5 };
                _currentParagraph = null;
                _styleStack.Push(new PdfTextStyle { Bold = true });
                break;
            case "td":
                _currentCell = new PdfTableCellModel { PaddingPt = 7.5 };
                _currentParagraph = null;
                break;

            // ── Inline formatting (with display:block CSS override) ────────
            case "strong":
            case "b":
                if (display == "block") FlushParagraph();
                _styleStack.Push(new PdfTextStyle { Bold = true });
                break;
            case "em":
            case "i":
                if (display == "block") FlushParagraph();
                _styleStack.Push(new PdfTextStyle { Italic = true });
                break;
            case "u":
                if (display == "block") FlushParagraph();
                _styleStack.Push(new PdfTextStyle { Underline = true });
                break;

            // ── Line break ───────────────────────────────────────────────
            case "br":
                if (_headingLevel > 0)
                    _headingText?.Append(' ');
                else
                {
                    EnsureParagraph();
                    _currentParagraph!.AddLineBreak();
                }
                break;

            // ── Horizontal rule ──────────────────────────────────────────
            case "hr":
                FlushParagraph();
                AddBlockToTarget(new PdfDividerModel());
                break;

            // ── Images ───────────────────────────────────────────────────
            case "img":
                HandleImage(element, css);
                break;

            // ── Block/inline elements with CSS-driven display ────────────
            default:
                if (display == "block")
                {
                    FlushParagraph();
                    var blockStyle = BuildTextStyleFromCss(css);
                    if (blockStyle != null)
                    {
                        _styleStack.Push(blockStyle);
                        _cssPushedElements.Add(_elementDepth);
                    }
                }
                break;
        }
    }

    public void LeaveElement(IElement element)
    {
        if (_skipDepth > 0)
        {
            _skipDepth--;
            return;
        }

        var tag = element.TagName.ToLowerInvariant();
        var css = _cssResolver.ResolveWithInheritance(element);
        var display = css.GetValueOrDefault("display") ?? CssValueParser.GetDefaultDisplay(tag);

        // ── Leaving a flex container ─────────────────────────────────────
        if (_flexStack.Count > 0 && _elementDepth == _flexStack.Peek().Depth)
        {
            LeaveFlexContainer();
        }

        // ── Leaving a direct child of a flex container ───────────────────
        if (_flexStack.Count > 0 && _elementDepth == _flexStack.Peek().Depth + 1)
        {
            FlushParagraph();
            if (_currentCell != null)
            {
                _flexStack.Peek().Row.Cells.Add(_currentCell);
                _currentCell = null;
            }
        }

        switch (tag)
        {
            // ── Headings ─────────────────────────────────────────────────
            case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                var headingText = _headingText?.ToString().Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(headingText))
                {
                    var heading = new PdfHeadingModel(headingText, _headingLevel)
                    {
                        Style = _headingStyle
                    };
                    AddBlockToTarget(heading);
                }
                _headingLevel = 0;
                _headingText = null;
                _headingStyle = null;
                break;

            // ── Tables ───────────────────────────────────────────────────
            case "table":
                if (_currentTable != null)
                {
                    AddBlockToTarget(_currentTable);
                    _currentTable = null;
                }
                break;
            case "tr":
                if (_currentRow != null && _currentTable != null)
                {
                    _currentTable.Rows.Add(_currentRow);
                    _currentRow = null;
                }
                break;
            case "th":
                FlushParagraph();
                if (_currentCell != null && _currentRow != null)
                {
                    _currentRow.Cells.Add(_currentCell);
                    _currentCell = null;
                }
                if (_styleStack.Count > 0) _styleStack.Pop();
                break;
            case "td":
                FlushParagraph();
                if (_currentCell != null && _currentRow != null)
                {
                    _currentRow.Cells.Add(_currentCell);
                    _currentCell = null;
                }
                break;

            // ── Inline formatting ────────────────────────────────────────
            case "strong":
            case "b":
            case "em":
            case "i":
            case "u":
                if (_styleStack.Count > 0) _styleStack.Pop();
                // Handle display:block CSS override → flush paragraph with style
                if (display == "block")
                {
                    if (_currentParagraph != null)
                    {
                        var mb = CssValueParser.ParseLength(css.GetValueOrDefault("margin-bottom"));
                        if (mb.HasValue)
                        {
                            _currentParagraph.Style ??= new PdfParagraphStyle();
                            _currentParagraph.Style.SpaceAfter = mb;
                        }
                    }
                    FlushParagraph();
                }
                break;

            // ── Block elements ───────────────────────────────────────────
            default:
                if (display == "block")
                {
                    FlushParagraph();
                    if (_cssPushedElements.Remove(_elementDepth) && _styleStack.Count > 0)
                        _styleStack.Pop();
                }
                break;
        }

        _elementDepth--;
    }

    public void VisitText(IText text)
    {
        if (_skipDepth > 0) return;

        var normalized = NormalizeWhitespace(text.Data);
        if (string.IsNullOrEmpty(normalized)) return;

        // Heading mode.
        if (_headingLevel > 0)
        {
            _headingText?.Append(normalized);
            return;
        }

        // Insignificant whitespace.
        if (string.IsNullOrWhiteSpace(normalized) && _currentParagraph == null)
            return;
        if (string.IsNullOrWhiteSpace(normalized) && _currentParagraph is { Inlines.Count: 0 })
            return;

        EnsureParagraph();
        ApplyCssStyleToParagraph(text);
        AddStyledText(normalized);
    }

    // ══════════════════════════ Flex Container Handling ═══════════════════

    private void EnterFlexContainer(IElement element, Dictionary<string, string> css)
    {
        var flexInfo = new FlexContainerInfo
        {
            LayoutTable = new PdfTableModel { IsLayoutTable = true },
            Row = new PdfTableRowModel(),
            Depth = _elementDepth,
            SavedParagraph = _currentParagraph,
            SavedTable = _currentTable,
            SavedRow = _currentRow,
            SavedCell = _currentCell,
        };

        // Detect margin-left: auto (right-aligned block).
        var marginLeft = css.GetValueOrDefault("margin-left");
        var margin = css.GetValueOrDefault("margin");
        var widthStr = css.GetValueOrDefault("width");
        var widthCm = CssValueParser.ParseLengthCm(widthStr);

        if ((marginLeft?.Trim() == "auto" ||
             CssValueParser.HasAutoMarginLeft(marginLeft, margin)) && widthCm.HasValue)
        {
            var indent = _contentWidthCm - widthCm.Value;
            if (indent > 0)
                flexInfo.LayoutTable.LeftIndentCm = indent;
            flexInfo.WidthCm = widthCm.Value;
        }
        else if (widthCm.HasValue)
        {
            flexInfo.WidthCm = widthCm.Value;
        }
        // Also check PARENT element's width + margin-left:auto
        // (handles cases like .totals div where positioning is on the parent).
        else if (element.ParentElement != null)
        {
            var parentCss = _cssResolver.Resolve(element.ParentElement);
            var parentWidthCm = CssValueParser.ParseLengthCm(parentCss.GetValueOrDefault("width"));
            var parentMarginLeft = parentCss.GetValueOrDefault("margin-left");
            var parentMargin = parentCss.GetValueOrDefault("margin");

            if (parentWidthCm.HasValue &&
                (parentMarginLeft?.Trim() == "auto" ||
                 CssValueParser.HasAutoMarginLeft(parentMarginLeft, parentMargin)))
            {
                var indent = _contentWidthCm - parentWidthCm.Value;
                if (indent > 0)
                    flexInfo.LayoutTable.LeftIndentCm = indent;
                flexInfo.WidthCm = parentWidthCm.Value;
            }
        }

        // Space before from margin.
        var marginTop = css.GetValueOrDefault("margin-top");
        var mt = CssValueParser.ParseLength(marginTop);
        if (!mt.HasValue && margin != null)
        {
            var ms = CssValueParser.ParseMarginShorthand(margin);
            mt = ms.Top > 0 ? ms.Top : null;
        }
        if (mt.HasValue)
            flexInfo.LayoutTable.SpaceBeforePt = mt;

        // Padding propagation.
        var padding = CssValueParser.ParseLength(css.GetValueOrDefault("padding"));
        if (padding.HasValue)
            flexInfo.DefaultCellPaddingPt = padding;

        _flexStack.Push(flexInfo);
        _currentParagraph = null;
        _currentTable = null;
        _currentRow = null;
        _currentCell = null;
    }

    private void LeaveFlexContainer()
    {
        var flex = _flexStack.Pop();
        FlushParagraph();

        if (flex.Row.Cells.Count > 0)
            flex.LayoutTable.Rows.Add(flex.Row);

        // Calculate column widths.
        var numCols = flex.Row.Cells.Count;
        if (numCols > 0)
        {
            var totalWidth = flex.WidthCm ?? _contentWidthCm;
            if (flex.LayoutTable.LeftIndentCm.HasValue)
                totalWidth = flex.WidthCm ?? (_contentWidthCm - flex.LayoutTable.LeftIndentCm.Value);
            flex.LayoutTable.ColumnWidthsCm = Enumerable.Repeat(totalWidth / numCols, numCols).ToList();
        }

        // Restore saved state.
        _currentParagraph = flex.SavedParagraph;
        _currentTable = flex.SavedTable;
        _currentRow = flex.SavedRow;
        _currentCell = flex.SavedCell;

        AddBlockToTarget(flex.LayoutTable);
    }

    // ══════════════════════════ Image Handling ════════════════════════════

    private void HandleImage(IElement element, Dictionary<string, string> css)
    {
        var src = element.GetAttribute("src");
        if (string.IsNullOrEmpty(src)) return;

        var resolvedPath = ResolveImagePath(src);
        if (string.IsNullOrEmpty(resolvedPath) || !File.Exists(resolvedPath)) return;

        var widthPt = CssValueParser.ParseLength(element.GetAttribute("width"))
                      ?? CssValueParser.ParseLength(css.GetValueOrDefault("width"));
        var heightPt = CssValueParser.ParseLength(element.GetAttribute("height"))
                       ?? CssValueParser.ParseLength(css.GetValueOrDefault("height"));

        EnsureParagraph();
        _currentParagraph!.Inlines.Add(new PdfInlineImageModel
        {
            Source = resolvedPath,
            WidthPt = widthPt,
            HeightPt = heightPt
        });
    }

    private string? ResolveImagePath(string src)
    {
        if (Path.IsPathRooted(src) && File.Exists(src))
            return src;

        if (!string.IsNullOrEmpty(_basePath))
        {
            var candidate = Path.Combine(_basePath, src);
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }

        if (File.Exists(src))
            return Path.GetFullPath(src);

        return null;
    }

    // ══════════════════════════ CSS Style Helpers ═════════════════════════

    private static PdfParagraphStyle? BuildParagraphStyle(Dictionary<string, string> css)
    {
        PdfParagraphStyle? style = null;
        void Ensure() => style ??= new PdfParagraphStyle();

        var align = CssValueParser.ParseAlignment(css.GetValueOrDefault("text-align"));
        if (align.HasValue) { Ensure(); style!.Alignment = align; }

        var fontSize = CssValueParser.ParseLength(css.GetValueOrDefault("font-size"));
        if (fontSize.HasValue) { Ensure(); style!.FontSize = fontSize; }

        var fontFamily = CssValueParser.ParseFontFamily(css.GetValueOrDefault("font-family"));
        if (fontFamily != null) { Ensure(); style!.FontName = fontFamily; }

        var color = CssValueParser.ParseColor(css.GetValueOrDefault("color"));
        if (color != null) { Ensure(); style!.FontColor = color; }

        if (CssValueParser.IsBold(css.GetValueOrDefault("font-weight"))) { Ensure(); style!.Bold = true; }
        if (CssValueParser.IsItalic(css.GetValueOrDefault("font-style"))) { Ensure(); style!.Italic = true; }

        var margin = css.GetValueOrDefault("margin");
        var mt = CssValueParser.ParseLength(css.GetValueOrDefault("margin-top"));
        var mb = CssValueParser.ParseLength(css.GetValueOrDefault("margin-bottom"));
        if (mt.HasValue) { Ensure(); style!.SpaceBefore = mt; }
        if (mb.HasValue) { Ensure(); style!.SpaceAfter = mb; }
        if (margin != null && !mt.HasValue && !mb.HasValue)
        {
            var m = CssValueParser.ParseMarginShorthand(margin);
            if (m.Top > 0) { Ensure(); style!.SpaceBefore = m.Top; }
            if (m.Bottom > 0) { Ensure(); style!.SpaceAfter = m.Bottom; }
        }

        return style;
    }

    private static PdfTextStyle? BuildTextStyleFromCss(Dictionary<string, string> css)
    {
        bool bold = CssValueParser.IsBold(css.GetValueOrDefault("font-weight"));
        bool italic = CssValueParser.IsItalic(css.GetValueOrDefault("font-style"));
        var fontSize = CssValueParser.ParseLength(css.GetValueOrDefault("font-size"));
        var color = CssValueParser.ParseColor(css.GetValueOrDefault("color"));

        if (!bold && !italic && !fontSize.HasValue && color == null)
            return null;

        return new PdfTextStyle
        {
            Bold = bold ? true : null,
            Italic = italic ? true : null,
            FontSize = fontSize,
            Color = color,
        };
    }

    private void ApplyCssStyleToParagraph(IText textNode)
    {
        if (_currentParagraph?.Style != null) return;

        var parent = textNode.ParentElement;
        if (parent == null) return;

        var css = _cssResolver.ResolveWithInheritance(parent);
        var style = BuildParagraphStyle(css);
        if (style != null && _currentParagraph != null)
            _currentParagraph.Style = style;
    }

    // ══════════════════════════ Common Helpers ════════════════════════════

    private void EnsureParagraph()
    {
        _currentParagraph ??= new PdfParagraphModel();
    }

    private PdfTextStyle? GetCurrentStyle()
    {
        if (_styleStack.Count == 0) return null;

        bool bold = false, italic = false, underline = false;
        double? fontSize = null;
        string? color = null;

        foreach (var s in _styleStack)
        {
            if (s.Bold == true) bold = true;
            if (s.Italic == true) italic = true;
            if (s.Underline == true) underline = true;
            if (s.FontSize.HasValue) fontSize ??= s.FontSize;
            if (s.Color != null) color ??= s.Color;
        }

        if (!bold && !italic && !underline && !fontSize.HasValue && color == null)
            return null;

        return new PdfTextStyle
        {
            Bold = bold ? true : null,
            Italic = italic ? true : null,
            Underline = underline ? true : null,
            FontSize = fontSize,
            Color = color,
        };
    }

    private void AddStyledText(string text)
    {
        _currentParagraph?.AddText(text, GetCurrentStyle());
    }

    private void FlushParagraph()
    {
        if (_currentParagraph == null || _currentParagraph.Inlines.Count == 0)
        {
            _currentParagraph = null;
            return;
        }

        if (_currentCell != null)
            _currentCell.Blocks.Add(_currentParagraph);
        else
            _currentSection.Blocks.Add(_currentParagraph);

        _currentParagraph = null;
    }

    private void AddBlockToTarget(PdfBlockModel block)
    {
        if (_currentCell != null)
        {
            _currentCell.Blocks.Add(block);
        }
        else if (_currentTable != null && block is PdfParagraphModel)
        {
            // Ignore paragraphs floating between table rows.
        }
        else
        {
            _currentSection.Blocks.Add(block);
        }
    }

    private static string NormalizeWhitespace(string text)
    {
        return WhitespaceRegex().Replace(text, " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    // ══════════════════════════ Inner Types ═══════════════════════════════

    private sealed class FlexContainerInfo
    {
        public PdfTableModel LayoutTable { get; init; } = new() { IsLayoutTable = true };
        public PdfTableRowModel Row { get; init; } = new();
        public int Depth { get; init; }
        public double? WidthCm { get; set; }
        public double? DefaultCellPaddingPt { get; set; }

        public PdfParagraphModel? SavedParagraph { get; init; }
        public PdfTableModel? SavedTable { get; init; }
        public PdfTableRowModel? SavedRow { get; init; }
        public PdfTableCellModel? SavedCell { get; init; }
    }
}
