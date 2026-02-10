using System.Net;
using System.Text;
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
        
        // Decode HTML entities
        text = WebUtility.HtmlDecode(text);
        
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
        var inScript = false;
        var inStyle = false;
        var tagName = new StringBuilder();
        var collectingTagName = false;

        for (int i = 0; i < html.Length; i++)
        {
            var c = html[i];
            
            if (c == '<')
            {
                inTag = true;
                collectingTagName = true;
                tagName.Clear();
                continue;
            }

            if (c == '>')
            {
                inTag = false;
                collectingTagName = false;
                
                // Check if we're entering or leaving script/style tags
                var tag = tagName.ToString().ToLowerInvariant().Trim();
                if (tag == "script")
                    inScript = true;
                else if (tag == "/script")
                    inScript = false;
                else if (tag == "style")
                    inStyle = true;
                else if (tag == "/style")
                    inStyle = false;
                
                tagName.Clear();
                continue;
            }

            if (inTag)
            {
                // Collect tag name to detect script/style tags
                if (collectingTagName && (char.IsLetter(c) || c == '/'))
                {
                    tagName.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    collectingTagName = false;
                }
                continue;
            }

            // Skip content inside script and style tags
            if (inScript || inStyle)
                continue;

            result.Append(c);
        }

        return result.ToString().Trim();
    }
}
