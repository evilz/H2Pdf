using RazorPdf.PdfVdom;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf.Translation.Translators;

internal class SectionTranslator : IVNodeTranslator
{
    public int Priority => 1;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfSection";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        ctx.CurrentSection = ctx.Document.AddSection();
        ctx.CurrentSection.PageSetup.PageFormat = ctx.Options.PageSize;
        ctx.CurrentSection.PageSetup.Orientation = ctx.Options.PageOrientation;
        ctx.CurrentSection.PageSetup.TopMargin = Unit.FromCentimeter(ctx.Options.MarginTop);
        ctx.CurrentSection.PageSetup.BottomMargin = Unit.FromCentimeter(ctx.Options.MarginBottom);
        ctx.CurrentSection.PageSetup.LeftMargin = Unit.FromCentimeter(ctx.Options.MarginLeft);
        ctx.CurrentSection.PageSetup.RightMargin = Unit.FromCentimeter(ctx.Options.MarginRight);
        ctx.CurrentParagraph = null;

        foreach (var child in element.Children)
            ctx.TranslateChild!(child, ctx);
    }
}
