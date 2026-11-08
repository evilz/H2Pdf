using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace H2Pdf.Tests;

public class HtmlPdfRendererTests
{
    [Fact]
    public void SaveToPdf_HtmlImage_EmbedsImageInPdf()
    {
        var renderer = new HtmlPdfRenderer();
        var document = renderer.Render("<p>Logo <img src='logo.png'></p>",
            new HtmlPdfOptions { BasePath = AppContext.BaseDirectory });
        var path = Path.Combine(Path.GetTempPath(), $"h2pdf-html-{Guid.NewGuid():N}.pdf");
        try
        {
            renderer.SaveToPdf(document, path);
            var pdf = System.Text.Encoding.Latin1.GetString(File.ReadAllBytes(path));
            Assert.Matches(@"/Subtype\s*/Image\b", pdf);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Render_BuildsDocumentWithHeadingAndTable()
    {
        var renderer = new HtmlPdfRenderer();
        var html = """
            <html>
              <head>
                <style>
                  h1 { text-align: center; }
                </style>
              </head>
              <body>
                <h1>Report</h1>
                <p>Intro paragraph</p>
                <table>
                  <thead><tr><th>Col A</th><th>Col B</th></tr></thead>
                  <tbody><tr><td>1</td><td>2</td></tr></tbody>
                </table>
              </body>
            </html>
            """;

        var options = new HtmlPdfOptions { ContentWidthCm = 16 };
        var document = renderer.Render(html, options);

        Assert.Single(document.Sections);
        var section = document.Sections[0];

        var heading = section.Elements.OfType<Paragraph>()
            .FirstOrDefault(p => p.Style == "Heading1");
        Assert.NotNull(heading);
        Assert.Equal(ParagraphAlignment.Center, heading!.Format.Alignment);

        var table = section.Elements.OfType<Table>().FirstOrDefault();
        Assert.NotNull(table);
        Assert.Equal(2, table!.Columns.Count);
        Assert.Equal(2, table.Rows.Count);
        Assert.True(table.Rows[0].HeadingFormat);

        var expectedColumnWidth = options.ContentWidthCm / 2;
        Assert.InRange(table.Columns[0].Width.Centimeter, expectedColumnWidth - 0.05, expectedColumnWidth + 0.05);
    }
}
