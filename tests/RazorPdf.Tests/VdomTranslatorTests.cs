using System.Text;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using RazorPdf;
using RazorPdf.PdfVdom;
using RazorPdf.Translation;

namespace RazorPdf.Tests;

public class VdomTranslatorTests
{
    // --- Heading tests ---

    [Theory]
    [InlineData(1, 24)]
    [InlineData(2, 18)]
    [InlineData(3, 14)]
    [InlineData(4, 12)]
    [InlineData(5, 10)]
    [InlineData(6, 8)]
    public void PdfHeading_Level_MapsToCorrectFontSize(int level, int expectedSize)
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var heading = builder.CreateElement("PdfHeading",
            new Dictionary<string, object?> { ["Level"] = level }, section);
        builder.AddText("Test Heading", heading);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var paragraph = (Paragraph)document.Sections[0].Elements[0];
        Assert.Equal(expectedSize, paragraph.Format.Font.Size.Point);
        Assert.True(paragraph.Format.Font.Bold);
    }

    [Fact]
    public void PdfHeading_HasSpacingBeforeAndAfter()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var heading = builder.CreateElement("PdfHeading",
            new Dictionary<string, object?> { ["Level"] = 1 }, section);
        builder.AddText("Heading", heading);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var paragraph = (Paragraph)document.Sections[0].Elements[0];
        Assert.Equal(12, paragraph.Format.SpaceBefore.Point);
        Assert.Equal(6, paragraph.Format.SpaceAfter.Point);
    }

    // --- Inline style stacking tests ---

    [Fact]
    public void BoldInsideItalic_StacksCorrectly()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var paragraph = builder.CreateElement("PdfParagraph", null, section);
        var italic = builder.CreateElement("PdfItalic", null, paragraph);
        var bold = builder.CreateElement("PdfBold", null, italic);
        builder.AddText("bold and italic", bold);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para = (Paragraph)document.Sections[0].Elements[0];
        var ft = (FormattedText)para.Elements[0];
        Assert.True(ft.Bold);
        Assert.True(ft.Italic);
    }

    [Fact]
    public void BoldAndUnderline_StacksCorrectly()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var paragraph = builder.CreateElement("PdfParagraph", null, section);
        var bold = builder.CreateElement("PdfBold", null, paragraph);
        var underline = builder.CreateElement("PdfUnderline", null, bold);
        builder.AddText("bold and underline", underline);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para = (Paragraph)document.Sections[0].Elements[0];
        var ft = (FormattedText)para.Elements[0];
        Assert.True(ft.Bold);
        Assert.Equal(Underline.Single, ft.Underline);
    }

    [Fact]
    public void NestedFormatting_RestoresOuterStyle()
    {
        // PdfBold has children: VText "bold", PdfItalic(VText "both"), VText "bold only"
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var paragraph = builder.CreateElement("PdfParagraph", null, section);
        var bold = builder.CreateElement("PdfBold", null, paragraph);
        builder.AddText("bold", bold);
        var italic = builder.CreateElement("PdfItalic", null, bold);
        builder.AddText("both", italic);
        builder.AddText("bold only", bold);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para = (Paragraph)document.Sections[0].Elements[0];

        // First element: "bold" with bold formatting
        var ft0 = (FormattedText)para.Elements[0];
        Assert.True(ft0.Bold);
        Assert.False(ft0.Italic);
        Assert.Equal("bold", GetFormattedTextContent(ft0));

        // Second element: "both" with bold+italic
        var ft1 = (FormattedText)para.Elements[1];
        Assert.True(ft1.Bold);
        Assert.True(ft1.Italic);
        Assert.Equal("both", GetFormattedTextContent(ft1));

        // Third element: "bold only" with bold, no italic
        var ft2 = (FormattedText)para.Elements[2];
        Assert.True(ft2.Bold);
        Assert.False(ft2.Italic);
        Assert.Equal("bold only", GetFormattedTextContent(ft2));
    }

    // --- List tests ---

    [Fact]
    public void UnorderedList_GeneratesBulletPrefixes()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var list = builder.CreateElement("PdfList",
            new Dictionary<string, object?> { ["Ordered"] = false }, section);
        var item1 = builder.CreateElement("PdfListItem", null, list);
        builder.AddText("Item 1", item1);
        var item2 = builder.CreateElement("PdfListItem", null, list);
        builder.AddText("Item 2", item2);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para1 = (Paragraph)document.Sections[0].Elements[0];
        Assert.Contains("\u2022 ", GetParagraphText(para1));
        Assert.Contains("Item 1", GetParagraphText(para1));

        var para2 = (Paragraph)document.Sections[0].Elements[1];
        Assert.Contains("\u2022 ", GetParagraphText(para2));
        Assert.Contains("Item 2", GetParagraphText(para2));
    }

    [Fact]
    public void OrderedList_GeneratesNumberPrefixes()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var list = builder.CreateElement("PdfList",
            new Dictionary<string, object?> { ["Ordered"] = true }, section);
        var item1 = builder.CreateElement("PdfListItem", null, list);
        builder.AddText("First", item1);
        var item2 = builder.CreateElement("PdfListItem", null, list);
        builder.AddText("Second", item2);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para1 = (Paragraph)document.Sections[0].Elements[0];
        Assert.Contains("1. ", GetParagraphText(para1));
        Assert.Contains("First", GetParagraphText(para1));

        var para2 = (Paragraph)document.Sections[0].Elements[1];
        Assert.Contains("2. ", GetParagraphText(para2));
        Assert.Contains("Second", GetParagraphText(para2));
    }

    // --- Table tests ---

    [Fact]
    public void Table_ColumnCountMatchesFirstRow()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var table = builder.CreateElement("PdfTable", null, section);
        var row = builder.CreateElement("PdfTableRow", null, table);
        builder.CreateElement("PdfTableCell", null, row);
        builder.CreateElement("PdfTableCell", null, row);
        builder.CreateElement("PdfTableCell", null, row);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var tbl = (Table)document.Sections[0].Elements[0];
        Assert.Equal(3, tbl.Columns.Count);
    }

    [Fact]
    public void Table_CellText_MapsCorrectly()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var table = builder.CreateElement("PdfTable", null, section);
        var row = builder.CreateElement("PdfTableRow", null, table);
        var cellA = builder.CreateElement("PdfTableCell", null, row);
        builder.AddText("A", cellA);
        var cellB = builder.CreateElement("PdfTableCell", null, row);
        builder.AddText("B", cellB);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var tbl = (Table)document.Sections[0].Elements[0];
        var cell0 = tbl.Rows[0].Cells[0];
        var cell1 = tbl.Rows[0].Cells[1];

        var para0 = (Paragraph)cell0.Elements[0];
        Assert.Equal("A", GetParagraphText(para0));

        var para1 = (Paragraph)cell1.Elements[0];
        Assert.Equal("B", GetParagraphText(para1));
    }

    [Fact]
    public void Table_HeaderRow_HasShading()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var table = builder.CreateElement("PdfTable", null, section);
        var row = builder.CreateElement("PdfTableRow",
            new Dictionary<string, object?> { ["IsHeader"] = true }, table);
        var cell = builder.CreateElement("PdfTableCell", null, row);
        builder.AddText("Header", cell);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var tbl = (Table)document.Sections[0].Elements[0];
        var headerRow = tbl.Rows[0];
        Assert.True(headerRow.HeadingFormat);
        Assert.Equal(Colors.LightGray, headerRow.Shading.Color);
    }

    [Fact]
    public void Table_HeaderRow_IsBold()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var table = builder.CreateElement("PdfTable", null, section);
        var row = builder.CreateElement("PdfTableRow",
            new Dictionary<string, object?> { ["IsHeader"] = true }, table);
        var cell = builder.CreateElement("PdfTableCell", null, row);
        builder.AddText("Header", cell);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var tbl = (Table)document.Sections[0].Elements[0];
        Assert.True(tbl.Rows[0].Format.Font.Bold);
    }

    // --- Image tests ---

    [Fact]
    public void Image_ByteArray_RendersOk()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        // Minimal valid 1x1 PNG
        var pngBytes = CreateMinimalPng();
        builder.CreateElement("PdfImage",
            new Dictionary<string, object?> { ["Source"] = pngBytes, ["Alt"] = "test" }, section);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        Assert.Single(document.Sections);
        Assert.True(document.Sections[0].Elements.Count > 0);
    }

    [Fact]
    public void Image_InvalidPath_DoesNotCrash()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        builder.CreateElement("PdfImage",
            new Dictionary<string, object?>
            {
                ["Source"] = "/nonexistent/path.png",
                ["Alt"] = "missing"
            }, section);

        var translator = new VdomTranslator();
        var options = new PdfRenderOptions { ImageAllowlistDirectory = "/nonexistent" };
        var document = translator.Translate(builder.Build(), options);

        var para = (Paragraph)document.Sections[0].Elements[0];
        Assert.Contains("[missing]", GetParagraphText(para));
    }

    [Fact]
    public void Image_DisallowedPath_RejectedGracefully()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        builder.CreateElement("PdfImage",
            new Dictionary<string, object?>
            {
                ["Source"] = "/etc/passwd",
                ["Alt"] = "disallowed"
            }, section);

        var translator = new VdomTranslator();
        var options = new PdfRenderOptions { ImageAllowlistDirectory = "/safe/dir" };
        var document = translator.Translate(builder.Build(), options);

        var para = (Paragraph)document.Sections[0].Elements[0];
        Assert.Contains("[disallowed]", GetParagraphText(para));
    }

    [Fact]
    public void Image_RemoteUrl_Rejected()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        builder.CreateElement("PdfImage",
            new Dictionary<string, object?>
            {
                ["Source"] = "https://example.com/image.png",
                ["Alt"] = "remote"
            }, section);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para = (Paragraph)document.Sections[0].Elements[0];
        Assert.Contains("[remote]", GetParagraphText(para));
    }

    [Fact]
    public void Image_NoAllowlist_FileDisabled()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        builder.CreateElement("PdfImage",
            new Dictionary<string, object?>
            {
                ["Source"] = "some/path.png",
                ["Alt"] = "noallow"
            }, section);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para = (Paragraph)document.Sections[0].Elements[0];
        Assert.Contains("[noallow]", GetParagraphText(para));
    }

    // --- Additional element tests ---

    [Fact]
    public void PdfParagraph_WithText_CreatesParagraph()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var paragraph = builder.CreateElement("PdfParagraph", null, section);
        builder.AddText("Hello World", paragraph);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para = (Paragraph)document.Sections[0].Elements[0];
        Assert.Equal("Hello World", GetParagraphText(para));
    }

    [Fact]
    public void PdfLineBreak_AddsLineBreak()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var paragraph = builder.CreateElement("PdfParagraph", null, section);
        builder.AddText("Before", paragraph);
        builder.CreateElement("PdfLineBreak", null, paragraph);
        builder.AddText("After", paragraph);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para = (Paragraph)document.Sections[0].Elements[0];
        // Should have 3 elements: Text, LineBreak/Character, Text
        Assert.True(para.Elements.Count >= 3);
    }

    [Fact]
    public void PdfHorizontalRule_CreatesParagraphWithBorder()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        builder.CreateElement("PdfHorizontalRule", null, section);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        var para = (Paragraph)document.Sections[0].Elements[0];
        Assert.Equal(1, para.Format.Borders.Bottom.Width.Point);
        Assert.Equal(Colors.Black, para.Format.Borders.Bottom.Color);
    }

    [Fact]
    public void PdfSection_CreatesSection()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        builder.CreateElement("PdfSection", null, doc);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        Assert.Single(document.Sections);
    }

    [Fact]
    public void PdfDocument_TranslatesChildren()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var p1 = builder.CreateElement("PdfParagraph", null, section);
        builder.AddText("First", p1);
        var p2 = builder.CreateElement("PdfParagraph", null, section);
        builder.AddText("Second", p2);

        var translator = new VdomTranslator();
        var document = translator.Translate(builder.Build());

        Assert.Single(document.Sections);
        Assert.Equal(2, document.Sections[0].Elements.Count);
    }

    // --- Helpers ---

    private static string GetParagraphText(Paragraph paragraph)
    {
        var text = new StringBuilder();
        foreach (var element in paragraph.Elements)
        {
            if (element is Text t)
                text.Append(t.Content);
            else if (element is FormattedText ft)
            {
                foreach (var inner in ft.Elements)
                {
                    if (inner is Text innerText)
                        text.Append(innerText.Content);
                }
            }
        }
        return text.ToString();
    }

    private static string GetFormattedTextContent(FormattedText ft)
    {
        var text = new StringBuilder();
        foreach (var inner in ft.Elements)
        {
            if (inner is Text t)
                text.Append(t.Content);
        }
        return text.ToString();
    }

    private static byte[] CreateMinimalPng()
    {
        // Minimal 1x1 white PNG
        return [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // 1x1
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, // 8-bit RGB
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, // IDAT chunk
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
            0x00, 0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC,
            0x33, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, // IEND chunk
            0x44, 0xAE, 0x42, 0x60, 0x82
        ];
    }
}
