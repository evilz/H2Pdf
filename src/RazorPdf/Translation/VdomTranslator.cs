using RazorPdf.PdfVdom;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf.Translation;

/// <summary>
/// Orchestrates VDOM to MigraDoc translation using registered translators
/// </summary>
public class VdomTranslator
{
    private readonly List<IVNodeTranslator> _translators;

    public VdomTranslator(IEnumerable<IVNodeTranslator>? translators = null)
    {
        _translators = (translators?.ToList() ?? GetDefaultTranslators())
            .OrderBy(t => t.Priority)
            .ToList();
    }

    /// <summary>
    /// Translates a VDOM tree to a MigraDoc Document
    /// </summary>
    public Document Translate(VNode root, PdfRenderOptions? options = null)
    {
        var document = new Document();
        var ctx = new TranslationContext(document, options);
        ctx.TranslateChild = TranslateNode;

        TranslateNode(root, ctx);

        return document;
    }

    private void TranslateNode(VNode node, TranslationContext ctx)
    {
        foreach (var translator in _translators)
        {
            if (translator.CanTranslate(node))
            {
                translator.Translate(node, ctx);
                return;
            }
        }

        // If no translator found for an element, just translate children
        if (node is VElement element)
        {
            foreach (var child in element.Children)
            {
                TranslateNode(child, ctx);
            }
        }
    }

    private static List<IVNodeTranslator> GetDefaultTranslators()
    {
        return
        [
            new Translators.DocumentTranslator(),
            new Translators.SectionTranslator(),
            new Translators.ParagraphTranslator(),
            new Translators.HeadingTranslator(),
            new Translators.TextTranslator(),
            new Translators.PdfTextElementTranslator(),
            new Translators.BoldTranslator(),
            new Translators.ItalicTranslator(),
            new Translators.UnderlineTranslator(),
            new Translators.StrikeTranslator(),
            new Translators.SpanTranslator(),
            new Translators.LineBreakTranslator(),
            new Translators.HorizontalRuleTranslator(),
            new Translators.ListTranslator(),
            new Translators.ListItemTranslator(),
            new Translators.TableTranslator(),
            new Translators.TableRowTranslator(),
            new Translators.TableCellTranslator(),
            new Translators.ImageTranslator(),
        ];
    }
}
