using RazorPdf.PdfVdom;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;

namespace RazorPdf.Translation.Translators;

internal class ImageTranslator : IVNodeTranslator
{
    public int Priority => 10;
    public bool CanTranslate(VNode node) => node is VElement e && e.Name == "PdfImage";
    public void Translate(VNode node, TranslationContext ctx)
    {
        var element = (VElement)node;
        var source = element.GetAttribute<object>("Source");
        var alt = element.GetAttribute("Alt", "");

        if (source == null) return;

        var section = ctx.EnsureSection();

        if (source is byte[] imageBytes)
        {
            var paragraph = ctx.CurrentCell != null
                ? ctx.CurrentCell.AddParagraph()
                : section.AddParagraph();
            try
            {
                var imageSource = ImageSource.FromBinary(
                    "image",
                    () => imageBytes);
                var image = paragraph.AddImage(imageSource);

                ApplyDimensions(element, image);
            }
            catch
            {
                if (!string.IsNullOrEmpty(alt))
                    paragraph.AddText($"[{alt}]");
            }
            ctx.CurrentParagraph = null;
        }
        else if (source is string filePath)
        {
            if (filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var paragraph = ctx.CurrentCell != null
                    ? ctx.CurrentCell.AddParagraph()
                    : section.AddParagraph();
                paragraph.AddText(!string.IsNullOrEmpty(alt) ? $"[{alt}]" : "[Image: remote URLs not supported]");
                ctx.CurrentParagraph = null;
                return;
            }

            var allowDir = ctx.Options.ImageAllowlistDirectory;
            if (string.IsNullOrEmpty(allowDir))
            {
                var paragraph = ctx.CurrentCell != null
                    ? ctx.CurrentCell.AddParagraph()
                    : section.AddParagraph();
                paragraph.AddText(!string.IsNullOrEmpty(alt) ? $"[{alt}]" : "[Image: file-based images disabled]");
                ctx.CurrentParagraph = null;
                return;
            }

            var fullPath = Path.GetFullPath(filePath);
            var fullAllowDir = Path.GetFullPath(allowDir);
            if (!fullPath.StartsWith(fullAllowDir, StringComparison.OrdinalIgnoreCase))
            {
                var paragraph = ctx.CurrentCell != null
                    ? ctx.CurrentCell.AddParagraph()
                    : section.AddParagraph();
                paragraph.AddText(!string.IsNullOrEmpty(alt) ? $"[{alt}]" : "[Image: path not allowed]");
                ctx.CurrentParagraph = null;
                return;
            }

            if (!File.Exists(fullPath))
            {
                var paragraph = ctx.CurrentCell != null
                    ? ctx.CurrentCell.AddParagraph()
                    : section.AddParagraph();
                paragraph.AddText(!string.IsNullOrEmpty(alt) ? $"[{alt}]" : "[Image: file not found]");
                ctx.CurrentParagraph = null;
                return;
            }

            var imgParagraph = ctx.CurrentCell != null
                ? ctx.CurrentCell.AddParagraph()
                : section.AddParagraph();
            try
            {
                var imageSource = ImageSource.FromFile(fullPath);
                var image = imgParagraph.AddImage(imageSource);

                ApplyDimensions(element, image);
            }
            catch
            {
                if (!string.IsNullOrEmpty(alt))
                    imgParagraph.AddText($"[{alt}]");
            }
            ctx.CurrentParagraph = null;
        }
    }

    private static void ApplyDimensions(VElement element, MigraDocCore.DocumentObjectModel.Shapes.Image image)
    {
        var width = element.GetAttribute<double?>("Width");
        var height = element.GetAttribute<double?>("Height");
        if (width.HasValue)
            image.Width = Unit.FromPoint(width.Value);
        if (height.HasValue)
            image.Height = Unit.FromPoint(height.Value);
    }
}
