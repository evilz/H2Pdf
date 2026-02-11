using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class UnderlineTranslator : IVNodeTranslator
{
    public int Priority => 20;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfUnderline";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var wasUnderline = ctx.IsUnderline;
        ctx.IsUnderline = true;
        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
        ctx.IsUnderline = wasUnderline;
    }
}
