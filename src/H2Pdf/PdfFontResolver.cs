using FontResolution.PdfSharp;
using PdfSharp.Fonts;

namespace H2Pdf;

internal static class PdfFontResolver
{
    private static readonly object Sync = new();

    internal static void EnsureInitialized()
    {
        lock (Sync)
        {
            if (GlobalFontSettings.FontResolver is null)
                GlobalFontSettings.FontResolver = new FontResolverPdfSharp();
        }
    }
}
