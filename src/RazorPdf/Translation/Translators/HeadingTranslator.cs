using RazorPdf.PdfVdom;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf.Translation.Translators;

internal class HeadingTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfHeading";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var level = element.GetAttribute("Level", 1);
        var section = ctx.EnsureSection();

        Paragraph paragraph;
        if (ctx.CurrentCell != null)
            paragraph = ctx.CurrentCell.AddParagraph();
        else
            paragraph = section.AddParagraph();

        ctx.CurrentParagraph = paragraph;

        var fontSize = level switch
        {
            1 => 24,
            2 => 18,
            3 => 14,
            4 => 12,
            5 => 10,
            6 => 8,
            _ => 14
        };

        paragraph.Format.Font.Size = fontSize;
        paragraph.Format.Font.Bold = true;
        paragraph.Format.SpaceBefore = 12;
        paragraph.Format.SpaceAfter = 6;

        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);

        ctx.CurrentParagraph = null;
    }
}
