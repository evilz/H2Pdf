using RazorPdf.PdfVdom;

namespace RazorPdf.Tests;

public class PdfVdomBuilderTests
{
    [Fact]
    public void CreateElement_NoParent_CreatesRoot()
    {
        var builder = new PdfVdomBuilder();
        var element = builder.CreateElement("PdfDocument");

        Assert.NotNull(element);
        Assert.Equal("PdfDocument", element.Name);
    }

    [Fact]
    public void CreateElement_WithParent_AddsChild()
    {
        var builder = new PdfVdomBuilder();
        var root = builder.CreateElement("PdfDocument");
        var child = builder.CreateElement("PdfSection", null, root);

        Assert.Single(root.Children);
        Assert.Same(child, root.Children[0]);
    }

    [Fact]
    public void AddText_AddsVTextChild()
    {
        var builder = new PdfVdomBuilder();
        var root = builder.CreateElement("PdfParagraph");
        builder.AddText("Hello", root);

        Assert.Single(root.Children);
        var textNode = Assert.IsType<VText>(root.Children[0]);
        Assert.Equal("Hello", textNode.Text);
    }

    [Fact]
    public void Build_ReturnsRoot()
    {
        var builder = new PdfVdomBuilder();
        var root = builder.CreateElement("PdfDocument");

        var result = builder.Build();

        Assert.Same(root, result);
    }

    [Fact]
    public void Build_ThrowsWhenEmpty()
    {
        var builder = new PdfVdomBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void NestedElements_MaintainCorrectTreeStructure()
    {
        var builder = new PdfVdomBuilder();
        var doc = builder.CreateElement("PdfDocument");
        var section = builder.CreateElement("PdfSection", null, doc);
        var paragraph = builder.CreateElement("PdfParagraph", null, section);
        builder.AddText("Deep text", paragraph);

        Assert.Single(doc.Children);
        var sectionNode = Assert.IsType<VElement>(doc.Children[0]);
        Assert.Equal("PdfSection", sectionNode.Name);

        Assert.Single(sectionNode.Children);
        var paragraphNode = Assert.IsType<VElement>(sectionNode.Children[0]);
        Assert.Equal("PdfParagraph", paragraphNode.Name);

        Assert.Single(paragraphNode.Children);
        var textNode = Assert.IsType<VText>(paragraphNode.Children[0]);
        Assert.Equal("Deep text", textNode.Text);
    }
}
