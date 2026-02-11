using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class StrikeTranslator : IVNodeTranslator
{
    public int Priority => 20;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfStrike";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var wasStrike = ctx.IsStrikethrough;
        ctx.IsStrikethrough = true;
        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
        ctx.IsStrikethrough = wasStrike;
    }
}
