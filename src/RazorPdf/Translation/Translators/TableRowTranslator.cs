using RazorPdf.PdfVdom;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf.Translation.Translators;

internal class TableRowTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfTableRow";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var table = ctx.CurrentTable;
        if (table == null) return;

        var row = table.AddRow();
        var isHeader = element.GetAttribute("IsHeader", false);

        if (isHeader)
        {
            row.HeadingFormat = true;
            row.Shading.Color = Colors.LightGray;
            row.Format.Font.Bold = true;
        }

        ctx.PushRow(row);
        ctx.CurrentParagraph = null;

        var cellIndex = 0;
        foreach (var child in element.Children)
        {
            if (child is VElement ce && ce.Name == "PdfTableCell")
            {
                if (cellIndex < row.Cells.Count)
                {
                    var cell = row.Cells[cellIndex];
                    ctx.PushCell(cell);
                    ctx.CurrentParagraph = null;

                    var colSpan = ce.GetAttribute("ColSpan", 1);
                    if (colSpan > 1)
                        cell.MergeRight = colSpan - 1;

                    foreach (var cellChild in ce.Children)
                        ctx.TranslateChild!(cellChild, ctx);

                    ctx.PopCell();
                    ctx.CurrentParagraph = null;
                    cellIndex += colSpan;
                }
            }
            else
            {
                ctx.TranslateChild!(child, ctx);
            }
        }

        ctx.PopRow();
        ctx.CurrentParagraph = null;
    }
}
