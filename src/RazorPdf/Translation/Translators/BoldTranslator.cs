using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class BoldTranslator : IVNodeTranslator
{
    public int Priority => 20;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfBold";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var wasBold = ctx.IsBold;
        ctx.IsBold = true;
        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
        ctx.IsBold = wasBold;
    }
}
