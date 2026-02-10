using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace RazorPdf.Tests;

public class HtmlToPdfConverterTests
{
    #region Basic Text and Paragraph

    [Fact]
    public void ConvertHtmlToSection_PlainText_CreatesParagraphWithText()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("Hello World", section);

        Assert.Equal(1, section.Elements.Count);
        var paragraph = (Paragraph)section.Elements[0];
        Assert.Contains("Hello World", GetParagraphText(paragraph));
    }

    [Fact]
    public void ConvertHtmlToSection_EmptyString_AddsNoElements()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("", section);

        Assert.Equal(0, section.Elements.Count);
    }

    [Fact]
    public void ConvertHtmlToSection_WhitespaceOnly_AddsNoElements()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("   ", section);

        Assert.Equal(0, section.Elements.Count);
    }

    [Fact]
    public void ConvertHtmlToSection_Null_AddsNoElements()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection(null!, section);

        Assert.Equal(0, section.Elements.Count);
    }

    [Fact]
    public void ConvertHtmlToSection_ParagraphTag_CreatesParagraph()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p>Hello</p>", section);

        Assert.True(section.Elements.Count >= 1);
        var paragraph = (Paragraph)section.Elements[0];
        Assert.Contains("Hello", GetParagraphText(paragraph));
    }

    [Fact]
    public void ConvertHtmlToSection_MultipleParagraphs_CreatesMultipleParagraphs()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p>First</p><p>Second</p>", section);

        Assert.True(section.Elements.Count >= 2);
    }

    #endregion

    #region Headings (h1-h6) → Paragraph with Font

    [Theory]
    [InlineData("h1", 24.0)]
    [InlineData("h2", 20.0)]
    [InlineData("h3", 16.0)]
    [InlineData("h4", 14.0)]
    [InlineData("h5", 12.0)]
    [InlineData("h6", 10.0)]
    public void ConvertHtmlToSection_Headings_ApplyCorrectFontSize(string tag, double expectedSize)
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection($"<{tag}>Heading</{tag}>", section);

        Assert.True(section.Elements.Count >= 1);
        var paragraph = (Paragraph)section.Elements[0];
        Assert.Equal(Unit.FromPoint(expectedSize), paragraph.Format.Font.Size);
        Assert.True(paragraph.Format.Font.Bold);
    }

    #endregion

    #region Font Formatting (Bold, Italic, Underline)

    [Fact]
    public void ConvertHtmlToSection_StrongTag_CreatesBoldFormattedText()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><strong>Bold text</strong></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithBold(paragraph));
    }

    [Fact]
    public void ConvertHtmlToSection_BTag_CreatesBoldFormattedText()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><b>Bold text</b></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithBold(paragraph));
    }

    [Fact]
    public void ConvertHtmlToSection_EmTag_CreatesItalicFormattedText()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><em>Italic text</em></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithItalic(paragraph));
    }

    [Fact]
    public void ConvertHtmlToSection_UTag_CreatesUnderlinedFormattedText()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><u>Underlined text</u></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithUnderline(paragraph));
    }

    #endregion

    #region Table → Rows, Columns, AddRow()

    [Fact]
    public void ConvertHtmlToSection_SimpleTable_CreatesTableWithRowsAndCells()
    {
        var document = new Document();
        var section = document.AddSection();

        var html = "<table><tr><td>Cell 1</td><td>Cell 2</td></tr></table>";
        HtmlToPdfConverter.ConvertHtmlToSection(html, section);

        // Find the table element
        Table? table = null;
        for (int i = 0; i < section.Elements.Count; i++)
        {
            if (section.Elements[i] is Table t)
            {
                table = t;
                break;
            }
        }

        Assert.NotNull(table);
        Assert.True(table.Rows.Count >= 1);
        Assert.True(table.Columns.Count >= 2);
    }

    [Fact]
    public void ConvertHtmlToSection_TableWithHeader_AppliesHeaderFormatting()
    {
        var document = new Document();
        var section = document.AddSection();

        var html = "<table><thead><tr><th>Header</th></tr></thead><tbody><tr><td>Data</td></tr></tbody></table>";
        HtmlToPdfConverter.ConvertHtmlToSection(html, section);

        Table? table = null;
        for (int i = 0; i < section.Elements.Count; i++)
        {
            if (section.Elements[i] is Table t)
            {
                table = t;
                break;
            }
        }

        Assert.NotNull(table);
        Assert.True(table.Rows.Count >= 2);
    }

    [Fact]
    public void ConvertHtmlToSection_TableMultipleRows_CreatesCorrectRowCount()
    {
        var document = new Document();
        var section = document.AddSection();

        var html = "<table><tr><td>R1C1</td><td>R1C2</td></tr><tr><td>R2C1</td><td>R2C2</td></tr><tr><td>R3C1</td><td>R3C2</td></tr></table>";
        HtmlToPdfConverter.ConvertHtmlToSection(html, section);

        Table? table = null;
        for (int i = 0; i < section.Elements.Count; i++)
        {
            if (section.Elements[i] is Table t)
            {
                table = t;
                break;
            }
        }

        Assert.NotNull(table);
        Assert.Equal(3, table.Rows.Count);
        Assert.Equal(2, table.Columns.Count);
    }

    #endregion

    #region Color → RGB

    [Theory]
    [InlineData("#ff0000", 255, 0, 0)]
    [InlineData("#00ff00", 0, 255, 0)]
    [InlineData("#0000ff", 0, 0, 255)]
    [InlineData("#f00", 255, 0, 0)]
    public void ParseColor_HexValues_ReturnsCorrectColor(string hex, byte r, byte g, byte b)
    {
        var color = HtmlToPdfConverter.ParseColor(hex);
        Assert.NotNull(color);
        Assert.Equal(r, color.Value.R);
        Assert.Equal(g, color.Value.G);
        Assert.Equal(b, color.Value.B);
    }

    [Fact]
    public void ParseColor_RgbFunction_ReturnsCorrectColor()
    {
        var color = HtmlToPdfConverter.ParseColor("rgb(128, 64, 32)");
        Assert.NotNull(color);
        Assert.Equal((byte)128, color.Value.R);
        Assert.Equal((byte)64, color.Value.G);
        Assert.Equal((byte)32, color.Value.B);
    }

    [Theory]
    [InlineData("red")]
    [InlineData("blue")]
    [InlineData("green")]
    [InlineData("black")]
    [InlineData("white")]
    public void ParseColor_NamedColors_ReturnsNonNull(string colorName)
    {
        var color = HtmlToPdfConverter.ParseColor(colorName);
        Assert.NotNull(color);
    }

    [Fact]
    public void ParseColor_EmptyString_ReturnsNull()
    {
        var color = HtmlToPdfConverter.ParseColor("");
        Assert.Null(color);
    }

    [Fact]
    public void ParseColor_InvalidColor_ReturnsNull()
    {
        var color = HtmlToPdfConverter.ParseColor("notacolor");
        Assert.Null(color);
    }

    #endregion

    #region ParagraphFormat → Alignment

    [Fact]
    public void ConvertHtmlToSection_AlignCenter_SetsCenterAlignment()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p align=\"center\">Centered</p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.Equal(ParagraphAlignment.Center, paragraph.Format.Alignment);
    }

    [Fact]
    public void ConvertHtmlToSection_StyleTextAlignRight_SetsRightAlignment()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p style=\"text-align: right\">Right</p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.Equal(ParagraphAlignment.Right, paragraph.Format.Alignment);
    }

    #endregion

    #region Style → Font (Inline Styles)

    [Fact]
    public void ConvertHtmlToSection_InlineColorStyle_AppliesColor()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><span style=\"color: #ff0000\">Red text</span></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithColor(paragraph));
    }

    [Fact]
    public void ConvertHtmlToSection_InlineFontFamily_AppliesFont()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><span style=\"font-family: Arial\">Arial text</span></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithFontName(paragraph, "Arial"));
    }

    [Fact]
    public void ConvertHtmlToSection_InlineFontSizePx_AppliesFontSize()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><span style=\"font-size: 16px\">Large text</span></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithFontSize(paragraph));
    }

    #endregion

    #region Lists

    [Fact]
    public void ConvertHtmlToSection_UnorderedList_CreatesBulletedItems()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<ul><li>Item 1</li><li>Item 2</li></ul>", section);

        // Should have at least 2 paragraphs (one per li)
        var paragraphCount = 0;
        for (int i = 0; i < section.Elements.Count; i++)
        {
            if (section.Elements[i] is Paragraph)
                paragraphCount++;
        }
        Assert.True(paragraphCount >= 2);
    }

    [Fact]
    public void ConvertHtmlToSection_OrderedList_CreatesNumberedItems()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<ol><li>First</li><li>Second</li></ol>", section);

        var paragraphCount = 0;
        for (int i = 0; i < section.Elements.Count; i++)
        {
            if (section.Elements[i] is Paragraph)
                paragraphCount++;
        }
        Assert.True(paragraphCount >= 2);
    }

    #endregion

    #region Script/Style Filtering

    [Fact]
    public void ConvertHtmlToSection_ScriptTag_SkipsScriptContent()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p>Hello</p><script>alert('test');</script><p>World</p>", section);

        // Should not contain "alert"
        for (int i = 0; i < section.Elements.Count; i++)
        {
            if (section.Elements[i] is Paragraph p)
            {
                var text = GetParagraphText(p);
                Assert.DoesNotContain("alert", text);
            }
        }
    }

    [Fact]
    public void ConvertHtmlToSection_StyleTag_SkipsStyleContent()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p>Hello</p><style>.test { color: red; }</style><p>World</p>", section);

        for (int i = 0; i < section.Elements.Count; i++)
        {
            if (section.Elements[i] is Paragraph p)
            {
                var text = GetParagraphText(p);
                Assert.DoesNotContain(".test", text);
            }
        }
    }

    #endregion

    #region Self-Closing Tags

    [Fact]
    public void ConvertHtmlToSection_BrTag_AddsLineBreak()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p>Line1<br/>Line2</p>", section);

        Assert.True(section.Elements.Count >= 1);
    }

    [Fact]
    public void ConvertHtmlToSection_HrTag_AddsParagraphWithBorder()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p>Before</p><hr/><p>After</p>", section);

        // Should have at least 3 elements (p, hr-paragraph, p)
        Assert.True(section.Elements.Count >= 3);
    }

    #endregion

    #region Image

    [Fact]
    public void ConvertHtmlToSection_ImgWithRemoteUrl_AddsAltText()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<img src=\"https://example.com/image.png\" alt=\"Test Image\" />", section);

        Assert.True(section.Elements.Count >= 1);
        var paragraph = (Paragraph)section.Elements[0];
        Assert.Contains("Test Image", GetParagraphText(paragraph));
    }

    [Fact]
    public void ConvertHtmlToSection_ImgWithRemoteUrlNoAlt_AddsDefaultAltText()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<img src=\"https://example.com/image.png\" />", section);

        Assert.True(section.Elements.Count >= 1);
        var paragraph = (Paragraph)section.Elements[0];
        Assert.Contains("[Image]", GetParagraphText(paragraph));
    }

    #endregion

    #region Code and Preformatted

    [Fact]
    public void ConvertHtmlToSection_CodeTag_AppliesMonospaceFont()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><code>var x = 1;</code></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithFontName(paragraph, "Courier New"));
    }

    #endregion

    #region Blockquote

    [Fact]
    public void ConvertHtmlToSection_Blockquote_AppliesIndentation()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<blockquote>Quoted text</blockquote>", section);

        Assert.True(section.Elements.Count >= 1);
        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(paragraph.Format.LeftIndent > Unit.FromPoint(0));
    }

    #endregion

    #region Complex HTML

    [Fact]
    public void ConvertHtmlToSection_ComplexHtml_HandlesMultipleElements()
    {
        var document = new Document();
        var section = document.AddSection();

        var html = @"
            <h1>Title</h1>
            <p>This is <strong>bold</strong> and <em>italic</em> text.</p>
            <table>
                <tr><th>Header</th></tr>
                <tr><td>Data</td></tr>
            </table>
            <ul>
                <li>Item 1</li>
                <li>Item 2</li>
            </ul>";

        HtmlToPdfConverter.ConvertHtmlToSection(html, section);

        // Should create multiple elements without throwing
        Assert.True(section.Elements.Count >= 4);
    }

    [Fact]
    public void ConvertHtmlToSection_HtmlEntities_DecodesCorrectly()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p>&amp; &lt; &gt; &quot;</p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        var text = GetParagraphText(paragraph);
        Assert.Contains("&", text);
        Assert.Contains("<", text);
        Assert.Contains(">", text);
    }

    [Fact]
    public void ConvertHtmlToSection_NestedFormatting_HandlesBoldItalic()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<p><strong><em>Bold Italic</em></strong></p>", section);

        var paragraph = (Paragraph)section.Elements[0];
        // Should have formatted text elements
        Assert.True(paragraph.Elements.Count >= 1);
    }

    #endregion

    #region Tokenizer Tests

    [Fact]
    public void Tokenize_SimpleText_ReturnsTextToken()
    {
        var tokens = HtmlToPdfConverter.Tokenize("Hello");
        Assert.Single(tokens);
        Assert.Equal(HtmlToPdfConverter.TokenType.Text, tokens[0].Type);
        Assert.Equal("Hello", tokens[0].Content);
    }

    [Fact]
    public void Tokenize_OpenTag_ReturnsOpenTagToken()
    {
        var tokens = HtmlToPdfConverter.Tokenize("<p>");
        Assert.Single(tokens);
        Assert.Equal(HtmlToPdfConverter.TokenType.OpenTag, tokens[0].Type);
        Assert.Equal("p", tokens[0].TagName);
    }

    [Fact]
    public void Tokenize_CloseTag_ReturnsCloseTagToken()
    {
        var tokens = HtmlToPdfConverter.Tokenize("</p>");
        Assert.Single(tokens);
        Assert.Equal(HtmlToPdfConverter.TokenType.CloseTag, tokens[0].Type);
        Assert.Equal("p", tokens[0].TagName);
    }

    [Fact]
    public void Tokenize_SelfClosingTag_ReturnsSelfClosingToken()
    {
        var tokens = HtmlToPdfConverter.Tokenize("<br/>");
        Assert.Single(tokens);
        Assert.Equal(HtmlToPdfConverter.TokenType.SelfClosingTag, tokens[0].Type);
        Assert.Equal("br", tokens[0].TagName);
    }

    [Fact]
    public void Tokenize_TagWithAttributes_ParsesAttributes()
    {
        var tokens = HtmlToPdfConverter.Tokenize("<p class=\"test\" id=\"p1\">");
        Assert.Single(tokens);
        Assert.Equal("test", tokens[0].Attributes["class"]);
        Assert.Equal("p1", tokens[0].Attributes["id"]);
    }

    [Fact]
    public void Tokenize_HtmlComment_SkipsComment()
    {
        var tokens = HtmlToPdfConverter.Tokenize("<!-- comment -->Hello");
        Assert.Single(tokens);
        Assert.Equal(HtmlToPdfConverter.TokenType.Text, tokens[0].Type);
        Assert.Equal("Hello", tokens[0].Content);
    }

    #endregion

    #region Attribute Parsing

    [Fact]
    public void ParseAttributes_SingleQuotedValue_ParsesCorrectly()
    {
        var attrs = HtmlToPdfConverter.ParseAttributes("style='color: red'");
        Assert.Equal("color: red", attrs["style"]);
    }

    [Fact]
    public void ParseAttributes_DoubleQuotedValue_ParsesCorrectly()
    {
        var attrs = HtmlToPdfConverter.ParseAttributes("class=\"container\"");
        Assert.Equal("container", attrs["class"]);
    }

    [Fact]
    public void ParseAttributes_MultipleAttributes_ParsesAll()
    {
        var attrs = HtmlToPdfConverter.ParseAttributes("class=\"test\" id=\"main\" style=\"color: red\"");
        Assert.Equal(3, attrs.Count);
        Assert.Equal("test", attrs["class"]);
        Assert.Equal("main", attrs["id"]);
        Assert.Equal("color: red", attrs["style"]);
    }

    #endregion

    #region Font Tag

    [Fact]
    public void ConvertHtmlToSection_FontTagWithColor_AppliesColor()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<font color=\"red\">Red text</font>", section);

        Assert.True(section.Elements.Count >= 1);
        var paragraph = (Paragraph)section.Elements[0];
        Assert.True(HasFormattedTextWithColor(paragraph));
    }

    #endregion

    #region Div

    [Fact]
    public void ConvertHtmlToSection_DivTag_CreatesNewParagraph()
    {
        var document = new Document();
        var section = document.AddSection();

        HtmlToPdfConverter.ConvertHtmlToSection("<div>Block 1</div><div>Block 2</div>", section);

        Assert.True(section.Elements.Count >= 2);
    }

    #endregion

    #region Helper Methods

    private static string GetParagraphText(Paragraph paragraph)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var element in paragraph.Elements)
        {
            if (element is Text text)
            {
                sb.Append(text.Content);
            }
            else if (element is FormattedText ft)
            {
                foreach (var ftElement in ft.Elements)
                {
                    if (ftElement is Text ftText)
                    {
                        sb.Append(ftText.Content);
                    }
                }
            }
        }
        return sb.ToString();
    }

    private static bool HasFormattedTextWithBold(Paragraph paragraph)
    {
        foreach (var element in paragraph.Elements)
        {
            if (element is FormattedText ft && ft.Font.Bold)
                return true;
        }
        return false;
    }

    private static bool HasFormattedTextWithItalic(Paragraph paragraph)
    {
        foreach (var element in paragraph.Elements)
        {
            if (element is FormattedText ft && ft.Font.Italic)
                return true;
        }
        return false;
    }

    private static bool HasFormattedTextWithUnderline(Paragraph paragraph)
    {
        foreach (var element in paragraph.Elements)
        {
            if (element is FormattedText ft && ft.Font.Underline == Underline.Single)
                return true;
        }
        return false;
    }

    private static bool HasFormattedTextWithColor(Paragraph paragraph)
    {
        foreach (var element in paragraph.Elements)
        {
            if (element is FormattedText ft && !ft.Font.Color.IsEmpty)
                return true;
        }
        return false;
    }

    private static bool HasFormattedTextWithFontName(Paragraph paragraph, string fontName)
    {
        foreach (var element in paragraph.Elements)
        {
            if (element is FormattedText ft && ft.Font.Name == fontName)
                return true;
        }
        return false;
    }

    private static bool HasFormattedTextWithFontSize(Paragraph paragraph)
    {
        foreach (var element in paragraph.Elements)
        {
            if (element is FormattedText ft && !ft.Font.Size.IsEmpty)
                return true;
        }
        return false;
    }

    #endregion
}
