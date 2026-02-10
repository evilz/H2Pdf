using System.Text;
using System.Text.Encodings.Web;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf;

/// <summary>
/// Utility class for converting HTML to MigraDoc elements
/// </summary>
public static class HtmlToPdfConverter
{
    /// <summary>
    /// Converts HTML string to MigraDoc section content
    /// </summary>
    /// <param name="html">HTML content to convert</param>
    /// <param name="section">Target section to add content to</param>
    public static void ConvertHtmlToSection(string html, Section section)
    {
        // This is a basic implementation
        // In a production system, you would use an HTML parser
        // to properly convert HTML elements to MigraDoc elements
        
        // Remove HTML tags for basic text extraction
        var text = RemoveHtmlTags(html);
        
        if (!string.IsNullOrWhiteSpace(text))
        {
            var paragraph = section.AddParagraph();
            paragraph.AddText(text);
        }
    }

    private static string RemoveHtmlTags(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var result = new StringBuilder();
        var inTag = false;

        foreach (var c in html)
        {
            if (c == '<')
            {
                inTag = true;
                continue;
            }

            if (c == '>')
            {
                inTag = false;
                continue;
            }

            if (!inTag)
            {
                result.Append(c);
            }
        }

        return HtmlEncoder.Default.Encode(result.ToString().Trim());
    }
}
