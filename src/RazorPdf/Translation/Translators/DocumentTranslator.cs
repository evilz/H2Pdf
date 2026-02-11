using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class DocumentTranslator : IVNodeTranslator
{
    public int Priority => 0;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfDocument";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
    }
}
