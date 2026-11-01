// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text.RegularExpressions;

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Turns the HTML of an email into its plain-text alternative: paragraphs and list items on their own lines, links as
/// <c>text (address)</c> and images as their alternative text.
/// </summary>
public static partial class PlainText
{
    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Comments { get; }

    [GeneratedRegex(@"<(?<tag>head|style|script|title)\b.*?</\k<tag>\s*>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Hidden { get; }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Whitespace { get; }

    [GeneratedRegex(@"<img\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Image { get; }

    [GeneratedRegex(@"(?<open><a\b[^>]*>)(?<inner>.*?)</a\s*>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Link { get; }

    [GeneratedRegex(@"<(?:ul|ol)\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ListStart { get; }

    [GeneratedRegex(@"<li\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ListItem { get; }

    [GeneratedRegex(@"<br\s*/?>|</li\s*>|</tr\s*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex LineBreak { get; }

    [GeneratedRegex(@"</(?:p|div|h[1-6]|table|ul|ol)\s*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BlockEnd { get; }

    [GeneratedRegex(@"<[^>]+>", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Tag { get; }

    [GeneratedRegex(@"\n{3,}", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ExtraBlankLines { get; }

    /// <summary>
    /// Converts HTML to plain text.
    /// </summary>
    /// <param name="html">The HTML of an email.</param>
    /// <returns>The plain text.</returns>
    public static string From(string html)
    {
        var text = Comments.Replace(html, string.Empty);
        text = Hidden.Replace(text, string.Empty);
        text = Whitespace.Replace(text, " ");
        text = Image.Replace(text, match => AttributeValue(match.Value, "alt"));
        text = Link.Replace(text, match => LinkAsText(match.Groups["open"].Value, match.Groups["inner"].Value));
        text = ListStart.Replace(ListItem.Replace(text, "- "), "\n");
        text = LineBreak.Replace(text, "\n");
        text = BlockEnd.Replace(text, "\n\n");
        text = Tag.Replace(text, string.Empty);
        text = WebUtility.HtmlDecode(text);

        var lines = text.Replace(' ', ' ').Split('\n').Select(line => Whitespace.Replace(line, " ").Trim());
        return ExtraBlankLines.Replace(string.Join('\n', lines), "\n\n").Trim() + "\n";
    }

    static string LinkAsText(string tag, string inner)
    {
        var address = AttributeValue(tag, "href");
        var label = Tag.Replace(inner, string.Empty).Trim();
        if (address.Length == 0 || address.StartsWith('#') || address == label || address == $"mailto:{label}")
        {
            return label;
        }

        return label.Length == 0 ? address : $"{label} ({address})";
    }

    static string AttributeValue(string tag, string name)
    {
        var match = Regex.Match(tag, $"""\b{name}\s*=\s*(?:"([^"]*)"|'([^']*)')""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        if (!match.Success)
        {
            return string.Empty;
        }

        return match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
    }
}
