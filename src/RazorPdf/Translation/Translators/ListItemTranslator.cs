using RazorPdf.PdfVdom;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf.Translation.Translators;

internal class ListItemTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfListItem";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var section = ctx.EnsureSection();

        Paragraph paragraph;
        if (ctx.CurrentCell != null)
            paragraph = ctx.CurrentCell.AddParagraph();
        else
            paragraph = section.AddParagraph();

        ctx.CurrentParagraph = paragraph;

        // Add bullet or number prefix
        if (ctx.InList)
        {
            var counter = ctx.IncrementListCounter();
            var prefix = ctx.IsOrderedList ? $"{counter}. " : "\u2022 ";
            paragraph.AddText(prefix);
        }

        paragraph.Format.LeftIndent = 20;

        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);

        ctx.CurrentParagraph = null;
    }
}
