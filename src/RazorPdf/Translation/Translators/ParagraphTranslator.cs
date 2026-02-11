using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class ParagraphTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfParagraph";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var section = ctx.EnsureSection();

        if (ctx.CurrentCell != null)
            ctx.CurrentParagraph = ctx.CurrentCell.AddParagraph();
        else
            ctx.CurrentParagraph = section.AddParagraph();

        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);

        ctx.CurrentParagraph = null;
    }
}
