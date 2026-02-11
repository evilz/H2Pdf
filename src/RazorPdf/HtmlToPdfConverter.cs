using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;

namespace RazorPdf;

/// <summary>
/// Utility class for converting HTML to MigraDoc elements.
/// Maps HTML Document Object Model to MigraDoc Document Object Model:
/// - Document → Sections, Styles, Info
/// - Section → Headers, Footers, Paragraphs, Tables
/// - Paragraph → Format, AddText(), AddFormattedText()
/// - Table → Rows, Columns, Format, AddRow()
/// - Style → Name, ParagraphFormat, Font
/// - Color → RGB values
/// - ParagraphFormat → Alignment, Borders, Shading, Font
/// - Font → Name, Size, Bold, Italic, Color
/// - Image → AddImage()
/// </summary>
public static class HtmlToPdfConverter
{
    /// <summary>
    /// Converts HTML string to MigraDoc section content
    /// </summary>
    /// <param name="html">HTML content to convert</param>
    /// <param name="section">Target section to add content to</param>
    public static void ConvertHtmlToSection(string html, Section section)
    {
        if (string.IsNullOrWhiteSpace(html))
            return;

        var tokens = Tokenize(html);
        var context = new ConversionContext(section);
        ProcessTokens(tokens, context);

        // Flush any remaining buffered text
        context.FlushText();
    }

    /// <summary>
    /// Converts a PDF VDOM tree to MigraDoc section content.
    /// </summary>
    /// <param name="nodes">VDOM nodes to convert</param>
    /// <param name="section">Target section to add content to</param>
    internal static void ConvertVdomToSection(IReadOnlyList<PdfVdomNode>? nodes, Section section)
    {
        if (nodes == null || nodes.Count == 0)
            return;

        var tokens = new List<HtmlToken>();
        foreach (var node in nodes)
        {
            AppendTokens(node, tokens);
        }

        if (tokens.Count == 0)
            return;

        var context = new ConversionContext(section);
        ProcessTokens(tokens, context);
        context.FlushText();
    }

    #region Tokenizer

    internal enum TokenType
    {
        Text,
        OpenTag,
        CloseTag,
        SelfClosingTag
    }

    internal sealed class HtmlToken
    {
        public TokenType Type { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    internal static List<HtmlToken> Tokenize(string html)
    {
        var tokens = new List<HtmlToken>();
        var i = 0;
        var textBuffer = new StringBuilder();

        while (i < html.Length)
        {
            if (html[i] == '<')
            {
                // Flush accumulated text
                if (textBuffer.Length > 0)
                {
                    tokens.Add(new HtmlToken { Type = TokenType.Text, Content = textBuffer.ToString() });
                    textBuffer.Clear();
                }

                var tagEnd = html.IndexOf('>', i);
                if (tagEnd == -1)
                {
                    // Malformed HTML - treat rest as text
                    textBuffer.Append(html, i, html.Length - i);
                    break;
                }

                var tagContent = html.Substring(i + 1, tagEnd - i - 1).Trim();

                // Skip comments (<!-- ... -->)
                if (tagContent.StartsWith("!--"))
                {
                    var commentEnd = html.IndexOf("-->", i, StringComparison.Ordinal);
                    if (commentEnd != -1)
                    {
                        i = commentEnd + 3;
                        continue;
                    }
                }
                // Skip DOCTYPE
                else if (tagContent.StartsWith('!'))
                {
                    i = tagEnd + 1;
                    continue;
                }

                var token = ParseTag(tagContent);
                tokens.Add(token);

                i = tagEnd + 1;
            }
            else
            {
                textBuffer.Append(html[i]);
                i++;
            }
        }

        if (textBuffer.Length > 0)
        {
            tokens.Add(new HtmlToken { Type = TokenType.Text, Content = textBuffer.ToString() });
        }

        return tokens;
    }

    private static HtmlToken ParseTag(string tagContent)
    {
        var token = new HtmlToken();

        if (tagContent.EndsWith('/'))
        {
            token.Type = TokenType.SelfClosingTag;
            tagContent = tagContent[..^1].Trim();
        }
        else if (tagContent.StartsWith('/'))
        {
            token.Type = TokenType.CloseTag;
            tagContent = tagContent[1..].Trim();
            token.TagName = ExtractTagName(tagContent).ToLowerInvariant();
            return token;
        }
        else
        {
            token.Type = TokenType.OpenTag;
        }

        token.TagName = ExtractTagName(tagContent).ToLowerInvariant();

        // Self-closing tags like <br>, <hr>, <img>
        if (token.Type == TokenType.OpenTag && IsSelfClosingTag(token.TagName))
        {
            token.Type = TokenType.SelfClosingTag;
        }

        // Parse attributes
        var attrPart = tagContent.Length > token.TagName.Length
            ? tagContent[token.TagName.Length..].Trim()
            : string.Empty;
        if (!string.IsNullOrEmpty(attrPart))
        {
            token.Attributes = ParseAttributes(attrPart);
        }

        return token;
    }

    private static string ExtractTagName(string tagContent)
    {
        var sb = new StringBuilder();
        foreach (var c in tagContent)
        {
            if (char.IsWhiteSpace(c) || c == '/' || c == '>')
                break;
            sb.Append(c);
        }
        return sb.ToString();
    }

    private static bool IsSelfClosingTag(string tagName)
    {
        return tagName is "br" or "hr" or "img" or "input" or "meta" or "link";
    }

    internal static Dictionary<string, string> ParseAttributes(string attrString)
    {
        var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var i = 0;

        while (i < attrString.Length)
        {
            while (i < attrString.Length && char.IsWhiteSpace(attrString[i])) i++;
            if (i >= attrString.Length) break;

            var nameStart = i;
            while (i < attrString.Length && attrString[i] != '=' && !char.IsWhiteSpace(attrString[i])) i++;
            var name = attrString[nameStart..i].Trim();

            if (string.IsNullOrEmpty(name)) { i++; continue; }

            while (i < attrString.Length && char.IsWhiteSpace(attrString[i])) i++;

            if (i < attrString.Length && attrString[i] == '=')
            {
                i++; // Skip '='
                while (i < attrString.Length && char.IsWhiteSpace(attrString[i])) i++;

                string value;
                if (i < attrString.Length && (attrString[i] == '"' || attrString[i] == '\''))
                {
                    var quote = attrString[i];
                    i++;
                    var valueStart = i;
                    while (i < attrString.Length && attrString[i] != quote) i++;
                    value = attrString[valueStart..i];
                    if (i < attrString.Length) i++;
                }
                else
                {
                    var valueStart = i;
                    while (i < attrString.Length && !char.IsWhiteSpace(attrString[i])) i++;
                    value = attrString[valueStart..i];
                }

                attrs[name] = value;
            }
            else
            {
                attrs[name] = string.Empty;
            }
        }

        return attrs;
    }

    #endregion

    #region VDOM Tokenization

    private static void AppendTokens(PdfVdomNode node, List<HtmlToken> tokens)
    {
        switch (node)
        {
            case PdfVdomText textNode:
                if (!string.IsNullOrEmpty(textNode.Text))
                {
                    tokens.Add(new HtmlToken { Type = TokenType.Text, Content = textNode.Text });
                }
                break;

            case PdfVdomElement elementNode:
                var tagName = elementNode.TagName.ToLowerInvariant();
                var attributes = BuildAttributes(elementNode.Attributes);

                if (IsSelfClosingTag(tagName))
                {
                    tokens.Add(new HtmlToken
                    {
                        Type = TokenType.SelfClosingTag,
                        TagName = tagName,
                        Attributes = attributes
                    });
                    break;
                }

                tokens.Add(new HtmlToken
                {
                    Type = TokenType.OpenTag,
                    TagName = tagName,
                    Attributes = attributes
                });

                foreach (var child in elementNode.Children)
                {
                    AppendTokens(child, tokens);
                }

                tokens.Add(new HtmlToken
                {
                    Type = TokenType.CloseTag,
                    TagName = tagName
                });
                break;
        }
    }

    private static Dictionary<string, string> BuildAttributes(IReadOnlyDictionary<string, object?> attributes)
    {
        var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in attributes)
        {
            if (value == null)
                continue;

            var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (stringValue == null)
                continue;

            results[key] = stringValue;
        }

        return results;
    }

    #endregion

    #region Conversion Context

    private sealed class FormatState
    {
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool Strikethrough { get; set; }
        public Color? Color { get; set; }
        public string? FontName { get; set; }
        public double? FontSize { get; set; }

        public FormatState Clone() => new()
        {
            Bold = Bold,
            Italic = Italic,
            Underline = Underline,
            Strikethrough = Strikethrough,
            Color = Color,
            FontName = FontName,
            FontSize = FontSize
        };
    }

    private sealed class ConversionContext
    {
        public Section Section { get; }
        public Paragraph? CurrentParagraph { get; set; }
        public Table? CurrentTable { get; set; }
        public Row? CurrentRow { get; set; }
        public int CurrentCellIndex { get; set; }
        public bool InScript { get; set; }
        public bool InStyle { get; set; }
        public bool InListItem { get; set; }
        public int ListCounter { get; set; }
        public bool IsOrderedList { get; set; }
        public Stack<FormatState> FormatStack { get; } = new();
        public StringBuilder TextBuffer { get; } = new();
        public bool InTableHeader { get; set; }

        public ConversionContext(Section section)
        {
            Section = section;
            FormatStack.Push(new FormatState());
        }

        public FormatState CurrentFormat => FormatStack.Peek();

        public void FlushText()
        {
            if (TextBuffer.Length == 0)
                return;

            var text = WebUtility.HtmlDecode(TextBuffer.ToString());
            // Normalize whitespace (collapse multiple whitespace to single space)
            text = Regex.Replace(text, @"\s+", " ");

            TextBuffer.Clear();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (CurrentRow != null && CurrentCellIndex >= 0 && CurrentTable != null)
            {
                // We're inside a table cell
                var cell = CurrentRow.Cells[CurrentCellIndex];
                var para = cell.Elements.Count == 0
                    ? cell.AddParagraph()
                    : (Paragraph)cell.Elements[^1];
                AddFormattedText(para, text);
            }
            else
            {
                EnsureParagraph();
                AddFormattedText(CurrentParagraph!, text);
            }
        }

        public void EnsureParagraph()
        {
            CurrentParagraph ??= Section.AddParagraph();
        }

        public void AddFormattedText(Paragraph paragraph, string text)
        {
            var format = CurrentFormat;
            if (format.Bold || format.Italic || format.Underline || format.Strikethrough ||
                format.Color.HasValue || format.FontName != null || format.FontSize.HasValue)
            {
                var ft = paragraph.AddFormattedText(text);
                if (format.Bold) ft.Font.Bold = true;
                if (format.Italic) ft.Font.Italic = true;
                if (format.Underline) ft.Font.Underline = MigraDocCore.DocumentObjectModel.Underline.Single;
                if (format.Color.HasValue) ft.Font.Color = format.Color.Value;
                if (format.FontName != null) ft.Font.Name = format.FontName;
                if (format.FontSize.HasValue) ft.Font.Size = Unit.FromPoint(format.FontSize.Value);
            }
            else
            {
                paragraph.AddText(text);
            }
        }
    }

    #endregion

    #region Token Processing

    private static void ProcessTokens(List<HtmlToken> tokens, ConversionContext context)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            // Skip script/style content
            if (context.InScript || context.InStyle)
            {
                if (token.Type == TokenType.CloseTag)
                {
                    if (token.TagName == "script") context.InScript = false;
                    if (token.TagName == "style") context.InStyle = false;
                }
                continue;
            }

            switch (token.Type)
            {
                case TokenType.Text:
                    context.TextBuffer.Append(token.Content);
                    break;

                case TokenType.OpenTag:
                    ProcessOpenTag(token, context, tokens, i);
                    break;

                case TokenType.CloseTag:
                    ProcessCloseTag(token, context);
                    break;

                case TokenType.SelfClosingTag:
                    ProcessSelfClosingTag(token, context);
                    break;
            }
        }
    }

    private static void ProcessOpenTag(HtmlToken token, ConversionContext context, List<HtmlToken> tokens, int currentIndex)
    {
        switch (token.TagName)
        {
            case "script":
                context.InScript = true;
                break;

            case "style":
                context.InStyle = true;
                break;

            // Headings (h1-h6) → Paragraph with heading style and appropriate font
            case "h1":
            case "h2":
            case "h3":
            case "h4":
            case "h5":
            case "h6":
                context.FlushText();
                context.CurrentParagraph = context.Section.AddParagraph();
                var headingLevel = int.Parse(token.TagName[1..]);
                ApplyHeadingFormat(context.CurrentParagraph, headingLevel);
                var headingFormat = context.CurrentFormat.Clone();
                headingFormat.Bold = true;
                context.FormatStack.Push(headingFormat);
                ApplyInlineStyle(token, context);
                break;

            // Paragraph → MigraDoc Paragraph with Format
            case "p":
                context.FlushText();
                context.CurrentParagraph = context.Section.AddParagraph();
                context.FormatStack.Push(context.CurrentFormat.Clone());
                ApplyInlineStyle(token, context);
                ApplyAlignment(token, context.CurrentParagraph);
                break;

            // Block-level div
            case "div":
                context.FlushText();
                context.CurrentParagraph = context.Section.AddParagraph();
                context.FormatStack.Push(context.CurrentFormat.Clone());
                ApplyInlineStyle(token, context);
                ApplyAlignment(token, context.CurrentParagraph);
                break;

            // Inline formatting → Font Bold, Italic, etc.
            case "strong":
            case "b":
                context.FlushText();
                var boldFormat = context.CurrentFormat.Clone();
                boldFormat.Bold = true;
                context.FormatStack.Push(boldFormat);
                break;

            case "em":
            case "i":
                context.FlushText();
                var italicFormat = context.CurrentFormat.Clone();
                italicFormat.Italic = true;
                context.FormatStack.Push(italicFormat);
                break;

            case "u":
                context.FlushText();
                var underlineFormat = context.CurrentFormat.Clone();
                underlineFormat.Underline = true;
                context.FormatStack.Push(underlineFormat);
                break;

            case "s":
            case "del":
            case "strike":
                context.FlushText();
                var strikeFormat = context.CurrentFormat.Clone();
                strikeFormat.Strikethrough = true;
                context.FormatStack.Push(strikeFormat);
                break;

            // Span with potential inline styling → Color, Font
            case "span":
                context.FlushText();
                var spanFormat = context.CurrentFormat.Clone();
                context.FormatStack.Push(spanFormat);
                ApplyInlineStyle(token, context);
                break;

            // Code → Monospace font
            case "code":
                context.FlushText();
                var codeFormat = context.CurrentFormat.Clone();
                codeFormat.FontName = "Courier New";
                context.FormatStack.Push(codeFormat);
                break;

            // Preformatted text
            case "pre":
                context.FlushText();
                context.CurrentParagraph = context.Section.AddParagraph();
                var preFormat = context.CurrentFormat.Clone();
                preFormat.FontName = "Courier New";
                context.FormatStack.Push(preFormat);
                break;

            // Hyperlinks
            case "a":
                context.FlushText();
                var linkFormat = context.CurrentFormat.Clone();
                linkFormat.Color = new Color(0, 0, 255);
                linkFormat.Underline = true;
                context.FormatStack.Push(linkFormat);
                break;

            // Lists → Paragraph with list markers
            case "ul":
                context.FlushText();
                context.IsOrderedList = false;
                context.ListCounter = 0;
                context.FormatStack.Push(context.CurrentFormat.Clone());
                break;

            case "ol":
                context.FlushText();
                context.IsOrderedList = true;
                context.ListCounter = 0;
                context.FormatStack.Push(context.CurrentFormat.Clone());
                break;

            case "li":
                context.FlushText();
                context.ListCounter++;
                context.InListItem = true;
                context.CurrentParagraph = context.Section.AddParagraph();
                context.CurrentParagraph.Format.LeftIndent = Unit.FromCentimeter(1);
                if (context.IsOrderedList)
                {
                    context.CurrentParagraph.AddText($"{context.ListCounter}. ");
                }
                else
                {
                    context.CurrentParagraph.AddText("\u2022 "); // Bullet character
                }
                context.FormatStack.Push(context.CurrentFormat.Clone());
                break;

            // Table → MigraDoc Table with Rows, Columns
            case "table":
                context.FlushText();
                context.CurrentParagraph = null;
                var colCount = CountTableColumns(tokens, currentIndex);
                if (colCount < 1) colCount = 1;
                context.CurrentTable = context.Section.AddTable();
                context.CurrentTable.Borders.Width = Unit.FromPoint(0.5);
                var colWidth = 16.0 / colCount;
                for (var c = 0; c < colCount; c++)
                {
                    context.CurrentTable.AddColumn(Unit.FromCentimeter(colWidth));
                }
                context.FormatStack.Push(context.CurrentFormat.Clone());
                break;

            case "thead":
                context.InTableHeader = true;
                break;

            case "tbody":
            case "tfoot":
                context.InTableHeader = false;
                break;

            case "tr":
                if (context.CurrentTable != null)
                {
                    context.CurrentRow = context.CurrentTable.AddRow();
                    context.CurrentCellIndex = -1;
                    if (context.InTableHeader)
                    {
                        context.CurrentRow.Shading.Color = new Color(230, 230, 230);
                    }
                }
                break;

            case "td":
            case "th":
                if (context.CurrentRow != null && context.CurrentTable != null)
                {
                    context.CurrentCellIndex++;
                    // Ensure we don't exceed column count
                    if (context.CurrentCellIndex < context.CurrentTable.Columns.Count)
                    {
                        var cellFormat = context.CurrentFormat.Clone();
                        if (token.TagName == "th")
                        {
                            cellFormat.Bold = true;
                        }
                        context.FormatStack.Push(cellFormat);
                        ApplyInlineStyle(token, context);
                    }
                    else
                    {
                        context.FormatStack.Push(context.CurrentFormat.Clone());
                    }
                }
                break;

            // Blockquote
            case "blockquote":
                context.FlushText();
                context.CurrentParagraph = context.Section.AddParagraph();
                context.CurrentParagraph.Format.LeftIndent = Unit.FromCentimeter(1.5);
                context.CurrentParagraph.Format.Borders.Left.Width = Unit.FromPoint(2);
                context.CurrentParagraph.Format.Borders.Left.Color = new Color(180, 180, 180);
                context.FormatStack.Push(context.CurrentFormat.Clone());
                break;

            // Font tag → Font Name, Size, Color
            case "font":
                context.FlushText();
                var fontFormat = context.CurrentFormat.Clone();
                if (token.Attributes.TryGetValue("color", out var fontColor))
                {
                    var parsedColor = ParseColor(fontColor);
                    if (parsedColor.HasValue) fontFormat.Color = parsedColor.Value;
                }
                if (token.Attributes.TryGetValue("face", out var fontFace))
                {
                    fontFormat.FontName = fontFace;
                }
                if (token.Attributes.TryGetValue("size", out var fontSize) && double.TryParse(fontSize, out var fSize))
                {
                    fontFormat.FontSize = HtmlFontSizeToPoints(fSize);
                }
                context.FormatStack.Push(fontFormat);
                break;

            default:
                if (IsBlockElement(token.TagName))
                {
                    context.FlushText();
                    context.CurrentParagraph = context.Section.AddParagraph();
                }
                context.FormatStack.Push(context.CurrentFormat.Clone());
                break;
        }
    }

    private static void ProcessCloseTag(HtmlToken token, ConversionContext context)
    {
        context.FlushText();

        switch (token.TagName)
        {
            case "h1":
            case "h2":
            case "h3":
            case "h4":
            case "h5":
            case "h6":
            case "p":
            case "div":
            case "blockquote":
            case "pre":
                PopFormat(context);
                context.CurrentParagraph = null;
                break;

            case "strong":
            case "b":
            case "em":
            case "i":
            case "u":
            case "s":
            case "del":
            case "strike":
            case "span":
            case "code":
            case "a":
            case "font":
                PopFormat(context);
                break;

            case "li":
                PopFormat(context);
                context.InListItem = false;
                context.CurrentParagraph = null;
                break;

            case "ul":
            case "ol":
                PopFormat(context);
                context.ListCounter = 0;
                break;

            case "table":
                PopFormat(context);
                context.CurrentTable = null;
                context.CurrentRow = null;
                context.CurrentCellIndex = -1;
                break;

            case "tr":
                context.CurrentRow = null;
                context.CurrentCellIndex = -1;
                break;

            case "td":
            case "th":
                PopFormat(context);
                break;

            case "thead":
                context.InTableHeader = false;
                break;

            default:
                PopFormat(context);
                break;
        }
    }

    private static void ProcessSelfClosingTag(HtmlToken token, ConversionContext context)
    {
        switch (token.TagName)
        {
            case "br":
                context.FlushText();
                if (context.CurrentRow != null && context.CurrentCellIndex >= 0 && context.CurrentTable != null)
                {
                    var cell = context.CurrentRow.Cells[context.CurrentCellIndex];
                    if (cell.Elements.Count > 0)
                    {
                        cell.AddParagraph();
                    }
                }
                else
                {
                    context.EnsureParagraph();
                    context.CurrentParagraph!.AddLineBreak();
                }
                break;

            case "hr":
                context.FlushText();
                context.CurrentParagraph = context.Section.AddParagraph();
                context.CurrentParagraph.Format.Borders.Bottom.Width = Unit.FromPoint(1);
                context.CurrentParagraph.Format.Borders.Bottom.Color = Colors.Black;
                context.CurrentParagraph.Format.SpaceAfter = Unit.FromPoint(6);
                context.CurrentParagraph = null;
                break;

            // Image → MigraDoc AddImage()
            case "img":
                context.FlushText();
                if (token.Attributes.TryGetValue("src", out var src) && !string.IsNullOrEmpty(src))
                {
                    // Only support local file paths or base64 data URIs for images
                    if (src.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) ||
                        (!src.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                         !src.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            context.EnsureParagraph();
                            var imageSource = ImageSource.FromFile(src);
                            var image = context.CurrentParagraph!.AddImage(imageSource);

                            if (token.Attributes.TryGetValue("width", out var width))
                            {
                                var parsedWidth = ParseDimension(width);
                                if (parsedWidth.HasValue) image.Width = parsedWidth.Value;
                            }
                            if (token.Attributes.TryGetValue("height", out var height))
                            {
                                var parsedHeight = ParseDimension(height);
                                if (parsedHeight.HasValue) image.Height = parsedHeight.Value;
                            }
                        }
                        catch
                        {
                            // If image loading fails, add alt text
                            var altText = token.Attributes.TryGetValue("alt", out var alt) ? alt : "[Image]";
                            context.EnsureParagraph();
                            context.CurrentParagraph!.AddText(altText);
                        }
                    }
                    else
                    {
                        // For remote URLs, show alt text
                        var altText = token.Attributes.TryGetValue("alt", out var alt) ? alt : "[Image]";
                        context.EnsureParagraph();
                        context.CurrentParagraph!.AddText(altText);
                    }
                }
                break;
        }
    }

    private static void PopFormat(ConversionContext context)
    {
        if (context.FormatStack.Count > 1)
        {
            context.FormatStack.Pop();
        }
    }

    #endregion

    #region Formatting Helpers

    /// <summary>
    /// Applies heading formatting to a paragraph based on heading level (1-6).
    /// Maps to MigraDoc ParagraphFormat and Font.
    /// </summary>
    private static void ApplyHeadingFormat(Paragraph paragraph, int level)
    {
        var fontSize = level switch
        {
            1 => 24.0,
            2 => 20.0,
            3 => 16.0,
            4 => 14.0,
            5 => 12.0,
            6 => 10.0,
            _ => 12.0
        };

        paragraph.Format.Font.Size = Unit.FromPoint(fontSize);
        paragraph.Format.Font.Bold = true;
        paragraph.Format.SpaceBefore = Unit.FromPoint(fontSize * 0.5);
        paragraph.Format.SpaceAfter = Unit.FromPoint(fontSize * 0.3);
    }

    /// <summary>
    /// Applies text alignment from HTML style or align attribute to MigraDoc ParagraphFormat.Alignment.
    /// </summary>
    private static void ApplyAlignment(HtmlToken token, Paragraph paragraph)
    {
        string? align = null;
        if (token.Attributes.TryGetValue("align", out var alignAttr))
        {
            align = alignAttr;
        }
        else if (token.Attributes.TryGetValue("style", out var style))
        {
            align = ExtractStyleValue(style, "text-align");
        }

        if (align != null)
        {
            paragraph.Format.Alignment = align.Trim().ToLowerInvariant() switch
            {
                "center" => ParagraphAlignment.Center,
                "right" => ParagraphAlignment.Right,
                "justify" => ParagraphAlignment.Justify,
                _ => ParagraphAlignment.Left
            };
        }
    }

    /// <summary>
    /// Applies inline CSS styles from the style attribute to the current format state.
    /// Handles color, font-family, font-size, font-weight, font-style, text-decoration.
    /// </summary>
    private static void ApplyInlineStyle(HtmlToken token, ConversionContext context)
    {
        if (!token.Attributes.TryGetValue("style", out var style) || string.IsNullOrEmpty(style))
            return;

        var format = context.CurrentFormat;

        var colorValue = ExtractStyleValue(style, "color");
        if (colorValue != null)
        {
            var parsedColor = ParseColor(colorValue);
            if (parsedColor.HasValue) format.Color = parsedColor.Value;
        }

        var bgColor = ExtractStyleValue(style, "background-color");
        if (bgColor != null && context.CurrentParagraph != null)
        {
            var parsedBg = ParseColor(bgColor);
            if (parsedBg.HasValue)
            {
                context.CurrentParagraph.Format.Shading.Color = parsedBg.Value;
            }
        }

        var fontFamily = ExtractStyleValue(style, "font-family");
        if (fontFamily != null)
        {
            var firstFont = fontFamily.Split(',')[0].Trim().Trim('\'', '"');
            format.FontName = firstFont;
        }

        var fontSizeStr = ExtractStyleValue(style, "font-size");
        if (fontSizeStr != null)
        {
            var parsedSize = ParseFontSize(fontSizeStr);
            if (parsedSize.HasValue) format.FontSize = parsedSize.Value;
        }

        var fontWeight = ExtractStyleValue(style, "font-weight");
        if (fontWeight != null)
        {
            format.Bold = fontWeight.Trim().ToLowerInvariant() is "bold" or "bolder" or "700" or "800" or "900";
        }

        var fontStyle = ExtractStyleValue(style, "font-style");
        if (fontStyle != null)
        {
            format.Italic = fontStyle.Trim().ToLowerInvariant() is "italic" or "oblique";
        }

        var textDecoration = ExtractStyleValue(style, "text-decoration");
        if (textDecoration != null)
        {
            var decLower = textDecoration.Trim().ToLowerInvariant();
            if (decLower.Contains("underline")) format.Underline = true;
            if (decLower.Contains("line-through")) format.Strikethrough = true;
        }
    }

    /// <summary>
    /// Parses CSS color values (hex, rgb(), named colors) to MigraDoc Color.
    /// </summary>
    internal static Color? ParseColor(string colorStr)
    {
        if (string.IsNullOrWhiteSpace(colorStr))
            return null;

        colorStr = colorStr.Trim();

        // Hex colors: #RGB, #RRGGBB
        if (colorStr.StartsWith('#'))
        {
            var hex = colorStr[1..];
            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }

            if (hex.Length == 6 &&
                byte.TryParse(hex[..2], System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex[2..4], System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex[4..6], System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                return new Color(r, g, b);
            }
        }

        // rgb(r, g, b)
        var rgbMatch = Regex.Match(colorStr, @"rgb\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)");
        if (rgbMatch.Success)
        {
            return new Color(
                byte.Parse(rgbMatch.Groups[1].Value),
                byte.Parse(rgbMatch.Groups[2].Value),
                byte.Parse(rgbMatch.Groups[3].Value));
        }

        // Named colors
        return colorStr.ToLowerInvariant() switch
        {
            "black" => Colors.Black,
            "white" => Colors.White,
            "red" => Colors.Red,
            "green" => Colors.Green,
            "blue" => Colors.Blue,
            "yellow" => Colors.Yellow,
            "orange" => Colors.Orange,
            "purple" => new Color(128, 0, 128),
            "gray" or "grey" => Colors.Gray,
            "darkgray" or "darkgrey" => Colors.DarkGray,
            "lightgray" or "lightgrey" => Colors.LightGray,
            "navy" => new Color(0, 0, 128),
            "teal" => new Color(0, 128, 128),
            "maroon" => new Color(128, 0, 0),
            _ => null
        };
    }

    private static string? ExtractStyleValue(string style, string property)
    {
        var pattern = $@"(?:^|;)\s*{Regex.Escape(property)}\s*:\s*([^;]+)";
        var match = Regex.Match(style, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static double? ParseFontSize(string sizeStr)
    {
        sizeStr = sizeStr.Trim().ToLowerInvariant();

        if (sizeStr.EndsWith("px") && double.TryParse(sizeStr[..^2], out var px))
            return px * 0.75;
        if (sizeStr.EndsWith("pt") && double.TryParse(sizeStr[..^2], out var pt))
            return pt;
        if (sizeStr.EndsWith("em") && double.TryParse(sizeStr[..^2], out var em))
            return em * 12.0;
        if (sizeStr.EndsWith("rem") && double.TryParse(sizeStr[..^3], out var rem))
            return rem * 12.0;

        return sizeStr switch
        {
            "xx-small" => 7.0,
            "x-small" => 8.5,
            "small" => 10.0,
            "medium" => 12.0,
            "large" => 14.0,
            "x-large" => 18.0,
            "xx-large" => 24.0,
            _ => null
        };
    }

    private static double HtmlFontSizeToPoints(double htmlSize)
    {
        return htmlSize switch
        {
            1 => 8,
            2 => 10,
            3 => 12,
            4 => 14,
            5 => 18,
            6 => 24,
            7 => 36,
            _ => 12
        };
    }

    private static Unit? ParseDimension(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim().ToLowerInvariant();

        if (value.EndsWith("px") && double.TryParse(value[..^2], out var px))
            return Unit.FromPoint(px * 0.75);
        if (value.EndsWith("cm") && double.TryParse(value[..^2], out var cm))
            return Unit.FromCentimeter(cm);
        if (value.EndsWith("in") && double.TryParse(value[..^2], out var inches))
            return Unit.FromInch(inches);
        if (value.EndsWith("pt") && double.TryParse(value[..^2], out var pt))
            return Unit.FromPoint(pt);
        // Plain number assumed to be pixels
        if (double.TryParse(value, out var num))
            return Unit.FromPoint(num * 0.75);

        return null;
    }

    private static bool IsBlockElement(string tagName)
    {
        return tagName is "div" or "p" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6"
            or "blockquote" or "pre" or "section" or "article" or "header" or "footer"
            or "nav" or "main" or "aside" or "figure" or "figcaption" or "address";
    }

    /// <summary>
    /// Scans ahead from the table open tag to count the maximum number of columns (td/th) in any row.
    /// This is needed because MigraDoc requires columns to be defined before rows are added.
    /// </summary>
    private static int CountTableColumns(List<HtmlToken> tokens, int tableTokenIndex)
    {
        var maxCols = 0;
        var currentCols = 0;
        var depth = 0;

        for (var i = tableTokenIndex + 1; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if (t.Type == TokenType.OpenTag)
            {
                if (t.TagName == "table")
                {
                    depth++;
                }
                else if (depth == 0 && t.TagName == "tr")
                {
                    currentCols = 0;
                }
                else if (depth == 0 && (t.TagName == "td" || t.TagName == "th"))
                {
                    currentCols++;
                    if (currentCols > maxCols)
                        maxCols = currentCols;
                }
            }
            else if (t.Type == TokenType.CloseTag)
            {
                if (t.TagName == "table")
                {
                    if (depth == 0)
                        break;
                    depth--;
                }
            }
        }

        return maxCols;
    }

    #endregion
}
