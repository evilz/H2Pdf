using RazorPdf.PdfVdom;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace RazorPdf.Translation.Translators;

internal class TableTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfTable";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var section = ctx.EnsureSection();

        // Count columns from first row
        var columnCount = 0;
        foreach (var child in element.Children)
        {
            if (child is VElement row && row.Name == "PdfTableRow")
            {
                columnCount = row.Children.Count(c => c is VElement ce && ce.Name == "PdfTableCell");
                break;
            }
        }

        if (columnCount == 0) return;

        var table = section.AddTable();
        table.Borders.Width = ctx.Options.TableBorderWidth;
        ctx.TableColumnCount = columnCount;

        for (int i = 0; i < columnCount; i++)
        {
            table.AddColumn(Unit.FromCentimeter(16.0 / columnCount));
        }

        ctx.PushTable(table);
        ctx.CurrentParagraph = null;

        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);

        ctx.PopTable();
        ctx.CurrentParagraph = null;
    }
}
