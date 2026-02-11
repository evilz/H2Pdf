using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace RazorPdf.Tests;

public class PdfRenderingTests
{
    [Fact]
    public void BuildDocument_ModelWithHeadingAndParagraph_RendersContent()
    {
        var builder = new PdfDocumentBuilder();
        builder.AddSection()
            .AddHeading("Title", 2)
            .AddParagraph("Hello world");

        var model = builder.Build();
        var document = PdfDocumentModelRenderer.BuildDocument(model);

        Assert.Single(document.Sections);
        var section = document.Sections[0];
        Assert.True(section.Elements.Count >= 2);

        var heading = (Paragraph)section.Elements[0];
        Assert.Equal("Heading2", heading.Style);

        var paragraph = (Paragraph)section.Elements[1];
        Assert.Contains("Hello world", GetParagraphText(paragraph));
    }

    [Fact]
    public void BuildDocument_TableModel_CreatesRowsAndColumns()
    {
        var builder = new PdfDocumentBuilder();
        builder.AddSection()
            .AddTable(table =>
            {
                table.AddHeaderRow("Col A", "Col B");
                table.AddRow("1", "2");
            });

        var document = PdfDocumentModelRenderer.BuildDocument(builder.Build());
        var section = document.Sections[0];
        var table = section.Elements.OfType<Table>().FirstOrDefault();

        Assert.NotNull(table);
        Assert.Equal(2, table!.Columns.Count);
        Assert.Equal(2, table.Rows.Count);
        Assert.True(table.Rows[0].HeadingFormat);
    }

    [Fact]
    public async Task RenderToPdfAsync_ComponentAddsContent_ProducesDocument()
    {
        var services = new ServiceCollection();
        services.AddRazorPdf();
        using var serviceProvider = services.BuildServiceProvider();

        var pdfRenderer = serviceProvider.GetRequiredService<PdfRenderer>();
        var document = await pdfRenderer.RenderToPdfAsync<TestComponent>(new Dictionary<string, object?>
        {
            ["Message"] = "Rendered from component"
        });

        Assert.Single(document.Sections);
        var section = document.Sections[0];
        Assert.True(section.Elements.Count >= 2);
        var paragraph = (Paragraph)section.Elements[1];
        Assert.Contains("Rendered from component", GetParagraphText(paragraph));
    }

    private static string GetParagraphText(Paragraph paragraph)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var element in paragraph.Elements)
        {
            if (element is Text text)
            {
                sb.Append(text.Content);
            }
            else if (element is FormattedText formattedText)
            {
                foreach (var formattedElement in formattedText.Elements)
                {
                    if (formattedElement is Text formattedTextRun)
                    {
                        sb.Append(formattedTextRun.Content);
                    }
                }
            }
        }

        return sb.ToString();
    }

    private sealed class TestComponent : ComponentBase
    {
        [Inject]
        public PdfBuildContextAccessor BuildContextAccessor { get; set; } = default!;

        [Parameter]
        public string Message { get; set; } = string.Empty;

        protected override void OnInitialized()
        {
            var builder = BuildContextAccessor.GetRequiredContext().Builder;
            builder.AddSection()
                .AddHeading("Component Output", 1)
                .AddParagraph(Message);
        }
    }
}
