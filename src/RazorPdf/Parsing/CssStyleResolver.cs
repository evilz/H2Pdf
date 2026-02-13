using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace RazorPdf.Parsing;

/// <summary>
/// A simple CSS rule resolver. Parses CSS text from &lt;style&gt; blocks and resolves
/// applicable styles per element, including CSS inheritance for font/text properties.
/// </summary>
public sealed partial class CssStyleResolver
{
    private readonly List<CssRule> _rules = [];

    /// <summary>
    /// Parses CSS text (from a &lt;style&gt; element) and builds internal rule list.
    /// </summary>
    public void Parse(string cssText)
    {
        // Remove comments
        cssText = CssCommentRegex().Replace(cssText, "");

        // Extract @media print rules (include them since PDF is print-like)
        var printRules = new List<string>();
        cssText = MediaPrintRegex().Replace(cssText, match =>
        {
            printRules.Add(match.Groups[1].Value);
            return "";
        });

        // Remove other @media blocks
        cssText = MediaOtherRegex().Replace(cssText, "");

        // Parse regular rules
        ParseRules(cssText);

        // Parse print rules (added last so they can override)
        foreach (var printCss in printRules)
            ParseRules(printCss);
    }

    /// <summary>
    /// Resolves CSS properties for this element only (no inheritance).
    /// </summary>
    public Dictionary<string, string> Resolve(IElement element)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in _rules)
        {
            if (MatchesSelector(element, rule.Selector))
            {
                foreach (var (key, value) in rule.Properties)
                    result[key] = value;
            }
        }

        // Inline style (highest priority)
        var inlineStyle = element.GetAttribute("style");
        if (!string.IsNullOrEmpty(inlineStyle))
        {
            foreach (var (key, value) in ParseProperties(inlineStyle))
                result[key] = value;
        }

        return result;
    }

    /// <summary>
    /// Resolves CSS properties with inherited properties from ancestors.
    /// </summary>
    public Dictionary<string, string> ResolveWithInheritance(IElement element)
    {
        // Collect ancestor chain from root to element
        var chain = new List<IElement>();
        var current = element;
        while (current != null)
        {
            chain.Add(current);
            current = current.ParentElement;
        }
        chain.Reverse(); // root → element

        var inherited = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var ancestor in chain)
        {
            var style = Resolve(ancestor);
            foreach (var (key, value) in style)
            {
                if (ancestor == element || IsInheritedProperty(key))
                    inherited[key] = value;
            }
        }

        return inherited;
    }

    // ──────────────────────────── Parsing ─────────────────────────────────

    private void ParseRules(string cssText)
    {
        foreach (Match match in CssRuleRegex().Matches(cssText))
        {
            var selectors = match.Groups[1].Value.Trim();
            var propertiesText = match.Groups[2].Value.Trim();
            var properties = ParseProperties(propertiesText);

            foreach (var selector in selectors.Split(','))
            {
                var trimmed = selector.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    _rules.Add(new CssRule(trimmed, properties));
            }
        }
    }

    internal static Dictionary<string, string> ParseProperties(string text)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in text.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var colonIndex = prop.IndexOf(':');
            if (colonIndex > 0)
            {
                var key = prop[..colonIndex].Trim();
                var value = prop[(colonIndex + 1)..].Trim().TrimEnd('!').Replace("!important", "").Trim();
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    result[key] = value;
            }
        }
        return result;
    }

    // ──────────────────────────── Selector matching ──────────────────────

    private static bool MatchesSelector(IElement element, string selector)
    {
        var parts = selector.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 1)
            return MatchesSimpleSelector(element, parts[0]);

        // Descendant selector: last part matches element, earlier parts match ancestors
        if (!MatchesSimpleSelector(element, parts[^1]))
            return false;

        var el = element.ParentElement;
        var partIndex = parts.Length - 2;

        while (el != null && partIndex >= 0)
        {
            if (MatchesSimpleSelector(el, parts[partIndex]))
                partIndex--;
            el = el.ParentElement;
        }

        return partIndex < 0;
    }

    private static bool MatchesSimpleSelector(IElement element, string selector)
    {
        // Class selector: .classname
        if (selector.StartsWith('.'))
        {
            var className = selector[1..];
            return element.ClassList.Contains(className);
        }

        // Tag name selector
        return string.Equals(element.TagName, selector, StringComparison.OrdinalIgnoreCase);
    }

    // ──────────────────────────── Inheritance ────────────────────────────

    private static bool IsInheritedProperty(string property)
    {
        return property switch
        {
            "font-family" or "font-size" or "font-weight" or "font-style"
            or "color" or "text-align" or "line-height" or "letter-spacing"
            or "word-spacing" or "text-transform" or "visibility" => true,
            _ => false
        };
    }

    // ──────────────────────────── Regex patterns ─────────────────────────

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssCommentRegex();

    [GeneratedRegex(@"@media\s+print\s*\{((?:[^{}]*\{[^}]*\})*\s*)\}", RegexOptions.Singleline)]
    private static partial Regex MediaPrintRegex();

    [GeneratedRegex(@"@media[^{]*\{((?:[^{}]*\{[^}]*\})*\s*)\}", RegexOptions.Singleline)]
    private static partial Regex MediaOtherRegex();

    [GeneratedRegex(@"([^{}]+)\{([^}]*)\}")]
    private static partial Regex CssRuleRegex();
}

internal sealed record CssRule(string Selector, Dictionary<string, string> Properties);
