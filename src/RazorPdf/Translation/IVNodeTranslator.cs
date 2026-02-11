using RazorPdf.PdfVdom;

namespace RazorPdf.Translation;

/// <summary>
/// Interface for translating VDOM nodes to MigraDoc elements
/// </summary>
public interface IVNodeTranslator
{
    int Priority { get; }
    bool CanTranslate(VNode node);
    void Translate(VNode node, TranslationContext ctx);
}
