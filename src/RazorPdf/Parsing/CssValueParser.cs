using System.Globalization;
using System.Text.RegularExpressions;

namespace RazorPdf.Parsing;

/// <summary>
/// Utility methods for parsing CSS values into numeric values usable by MigraDoc.
/// </summary>
public static partial class CssValueParser
{
    /// <summary>Conversion factor from CSS px to PDF points (at 96 dpi).</summary>
    private const double PxToPoint = 72.0 / 96.0; // 0.75

    /// <summary>
    /// Parses a CSS length value (px, pt, em, cm, mm, in) into PDF points.
    /// Returns null if the value cannot be parsed.
    /// </summary>
    public static double? ParseLength(string? value, double parentFontSizePt = 12)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim().ToLowerInvariant();

        if (value == "0")
            return 0;

        if (value.EndsWith("px") && double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var px))
            return px * PxToPoint;

        if (value.EndsWith("pt") && double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var pt))
            return pt;

        if (value.EndsWith("em") && double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var em))
            return em * parentFontSizePt;

        if (value.EndsWith("rem") && double.TryParse(value[..^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var rem))
            return rem * 12; // rem based on default root font size

        if (value.EndsWith("cm") && double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var cm))
            return cm * 28.3465;

        if (value.EndsWith("mm") && double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var mm))
            return mm * 2.83465;

        if (value.EndsWith("in") && double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var inch))
            return inch * 72;

        // Bare number (assume px)
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var bare))
            return bare * PxToPoint;

        return null;
    }

    /// <summary>
    /// Parses a CSS length value into centimeters. Returns null if not parsable.
    /// </summary>
    public static double? ParseLengthCm(string? value, double parentFontSizePt = 12)
    {
        var pt = ParseLength(value, parentFontSizePt);
        return pt.HasValue ? pt.Value / 28.3465 : null;
    }

    /// <summary>
    /// Parses CSS font-weight into a boolean (true = bold).
    /// </summary>
    public static bool IsBold(string? fontWeight)
    {
        if (string.IsNullOrWhiteSpace(fontWeight))
            return false;

        fontWeight = fontWeight.Trim().ToLowerInvariant();
        if (fontWeight is "bold" or "bolder")
            return true;

        if (int.TryParse(fontWeight, out var weight))
            return weight >= 700;

        return false;
    }

    /// <summary>
    /// Parses CSS font-style into a boolean (true = italic).
    /// </summary>
    public static bool IsItalic(string? fontStyle)
    {
        return fontStyle?.Trim().ToLowerInvariant() is "italic" or "oblique";
    }

    /// <summary>
    /// Parses CSS text-align into a PdfAlignment value.
    /// </summary>
    public static PdfAlignment? ParseAlignment(string? textAlign)
    {
        return textAlign?.Trim().ToLowerInvariant() switch
        {
            "left" => PdfAlignment.Left,
            "right" => PdfAlignment.Right,
            "center" => PdfAlignment.Center,
            "justify" => PdfAlignment.Justify,
            _ => null
        };
    }

    /// <summary>
    /// Parses a CSS color value (hex, rgb, named) into a #RRGGBB hex string.
    /// Returns null if not parsable.
    /// </summary>
    public static string? ParseColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim().ToLowerInvariant();

        // Named colors (subset most used)
        var named = value switch
        {
            "black" => "#000000",
            "white" => "#ffffff",
            "red" => "#ff0000",
            "green" => "#008000",
            "blue" => "#0000ff",
            "gray" or "grey" => "#808080",
            "silver" => "#c0c0c0",
            "transparent" => null,
            "none" => null,
            _ => null
        };
        if (named != null || value is "transparent" or "none")
            return named;

        // #RGB
        if (value.Length == 4 && value[0] == '#')
        {
            return $"#{value[1]}{value[1]}{value[2]}{value[2]}{value[3]}{value[3]}";
        }

        // #RRGGBB
        if (value.Length == 7 && value[0] == '#')
            return value;

        // rgb(r, g, b)
        var rgbMatch = RgbRegex().Match(value);
        if (rgbMatch.Success &&
            int.TryParse(rgbMatch.Groups[1].Value, out var r) &&
            int.TryParse(rgbMatch.Groups[2].Value, out var g) &&
            int.TryParse(rgbMatch.Groups[3].Value, out var b))
        {
            return $"#{r:x2}{g:x2}{b:x2}";
        }

        // rgba(r, g, b, a) - ignore alpha
        var rgbaMatch = RgbaRegex().Match(value);
        if (rgbaMatch.Success &&
            int.TryParse(rgbaMatch.Groups[1].Value, out r) &&
            int.TryParse(rgbaMatch.Groups[2].Value, out g) &&
            int.TryParse(rgbaMatch.Groups[3].Value, out b))
        {
            return $"#{r:x2}{g:x2}{b:x2}";
        }

        return null;
    }

    /// <summary>
    /// Parses the CSS shorthand margin property and returns (top, right, bottom, left) in points.
    /// </summary>
    public static (double Top, double Right, double Bottom, double Left) ParseMarginShorthand(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (0, 0, 0, 0);

        var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var values = parts.Select(p => p == "auto" ? 0 : ParseLength(p) ?? 0).ToArray();

        return values.Length switch
        {
            1 => (values[0], values[0], values[0], values[0]),
            2 => (values[0], values[1], values[0], values[1]),
            3 => (values[0], values[1], values[2], values[1]),
            4 => (values[0], values[1], values[2], values[3]),
            _ => (0, 0, 0, 0)
        };
    }

    /// <summary>
    /// Checks if a margin value contains 'auto' (e.g., margin-left: auto for right-alignment).
    /// </summary>
    public static bool HasAutoMarginLeft(string? marginLeft, string? marginShorthand)
    {
        if (marginLeft?.Trim().ToLowerInvariant() == "auto")
            return true;

        if (!string.IsNullOrEmpty(marginShorthand))
        {
            var parts = marginShorthand.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // margin: top right bottom left
            // margin: top leftright bottom
            // Left is auto?
            if (parts.Length == 4 && parts[3].Trim().ToLowerInvariant() == "auto")
                return true;
            if (parts.Length is 2 or 3 && parts[1].Trim().ToLowerInvariant() == "auto")
                return true;
        }

        return false;
    }

    /// <summary>
    /// Parses a CSS font-family value and returns the first font name.
    /// </summary>
    public static string? ParseFontFamily(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Take the first font in the list
        var first = value.Split(',')[0].Trim().Trim('"', '\'');

        // Map generic families
        return first.ToLowerInvariant() switch
        {
            "sans-serif" => "Arial",
            "serif" => "Times New Roman",
            "monospace" => "Courier New",
            _ => first
        };
    }

    /// <summary>
    /// Parses a CSS border shorthand (e.g., "1px solid #ddd") into (thickness in pt, color hex).
    /// </summary>
    public static (double ThicknessPt, string? Color)? ParseBorder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().ToLowerInvariant() == "none")
            return null;

        var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        double thickness = 0.5;
        string? color = null;

        foreach (var part in parts)
        {
            var len = ParseLength(part);
            if (len.HasValue)
            {
                thickness = len.Value;
                continue;
            }

            var c = ParseColor(part);
            if (c != null)
            {
                color = c;
                continue;
            }
            // "solid", "dashed", etc. - ignored for MigraDoc
        }

        return (thickness, color);
    }

    /// <summary>
    /// Gets the default CSS display value for an HTML tag.
    /// </summary>
    public static string GetDefaultDisplay(string tag) => tag.ToLowerInvariant() switch
    {
        "div" or "p" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6"
        or "section" or "article" or "header" or "footer" or "aside"
        or "nav" or "main" or "table" or "form" or "fieldset"
        or "ul" or "ol" or "li" or "blockquote" or "pre" or "address"
        or "figure" or "figcaption" or "hr" or "dl" or "dd" or "dt"
        => "block",

        "span" or "a" or "strong" or "em" or "b" or "i" or "u"
        or "small" or "sub" or "sup" or "label" or "abbr" or "code"
        => "inline",

        "img" or "br" => "inline",

        "tr" => "table-row",
        "td" or "th" => "table-cell",
        "thead" or "tbody" or "tfoot" => "table-row-group",

        _ => "inline"
    };

    // ──────────────────────────── Regex patterns ─────────────────────────

    [GeneratedRegex(@"rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)")]
    private static partial Regex RgbRegex();

    [GeneratedRegex(@"rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*[\d.]+\s*\)")]
    private static partial Regex RgbaRegex();
}
