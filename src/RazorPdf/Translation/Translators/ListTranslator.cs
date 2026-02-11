using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class ListTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfList";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var ordered = element.GetAttribute("Ordered", false);
        ctx.PushList(ordered);

        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);

        ctx.PopList();
    }
}
