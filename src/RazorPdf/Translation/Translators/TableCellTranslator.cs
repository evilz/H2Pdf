using RazorPdf.PdfVdom;

namespace RazorPdf.Translation.Translators;

internal class TableCellTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfTableCell";
    public void Translate(VNode node, TranslationContext ctx)
    {
        // Cells are mainly handled by TableRowTranslator
        // This handles orphan cell elements (shouldn't normally occur)
        var element = (VElement)node;
        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
    }
}
