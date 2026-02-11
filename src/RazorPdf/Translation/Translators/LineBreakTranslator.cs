using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class LineBreakTranslator : IVNodeTranslator
{
    public int Priority => 30;
    public bool CanTranslate(VNode node) => node is VElement e && (e.Name == "PdfLineBreak" || e.Name == "PdfBr");
    public void Translate(VNode node, TranslationContext ctx)
    {
        var paragraph = ctx.EnsureParagraph();
        paragraph.AddLineBreak();
    }
}
