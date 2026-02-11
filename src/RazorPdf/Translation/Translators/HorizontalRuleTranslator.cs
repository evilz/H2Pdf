using RazorPdf.PdfVdom;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf.Translation.Translators;

internal class HorizontalRuleTranslator : IVNodeTranslator
{
    public int Priority => 30;
    public bool CanTranslate(VNode node) => node is VElement e && (e.Name == "PdfHorizontalRule" || e.Name == "PdfHr");
    public void Translate(VNode node, TranslationContext ctx)
    {
        var section = ctx.EnsureSection();
        Paragraph paragraph;
        if (ctx.CurrentCell != null)
            paragraph = ctx.CurrentCell.AddParagraph();
        else
            paragraph = section.AddParagraph();

        paragraph.Format.Borders.Bottom.Width = 1;
        paragraph.Format.Borders.Bottom.Color = Colors.Black;
        paragraph.Format.SpaceAfter = 6;
        ctx.CurrentParagraph = null;
    }
}
