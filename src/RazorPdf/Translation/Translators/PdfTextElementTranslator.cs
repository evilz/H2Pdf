using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class PdfTextElementTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfText";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
    }
}
