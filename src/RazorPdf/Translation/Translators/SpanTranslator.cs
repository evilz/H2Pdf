using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class SpanTranslator : IVNodeTranslator
{
    public int Priority => 20;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfSpan";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
    }
}
