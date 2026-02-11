using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class TextTranslator : IVNodeTranslator
{
    public int Priority => 100;
    public bool CanTranslate(VNode node) => node is VText;
    public void Translate(VNode node, TranslationContext ctx)
    {
        var text = (VText)node;
        ctx.AddFormattedText(text.Text);
    }
}
