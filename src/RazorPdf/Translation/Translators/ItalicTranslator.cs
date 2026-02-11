using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class ItalicTranslator : IVNodeTranslator
{
    public int Priority => 20;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfItalic";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var wasItalic = ctx.IsItalic;
        ctx.IsItalic = true;
        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
        ctx.IsItalic = wasItalic;
    }
}
