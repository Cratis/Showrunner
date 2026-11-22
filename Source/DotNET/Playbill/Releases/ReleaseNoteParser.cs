// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// Reads what is in a captured release note: its sections, a line that sums it up and the record of how the release was made.
/// </summary>
internal static partial class ReleaseNoteParser
{
    const int MaximumSummaryLength = 200;

    [GeneratedRegex(@"^\s{0,3}(?<marker>`{3,}|~{3,})", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex FenceExpression { get; }

    [GeneratedRegex(@"^##\s+(?<name>.+?)\s*#*\s*$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HeadingExpression { get; }

    [GeneratedRegex(@"^\s*(?:[-+*]|\d+[.)])\s+(?<text>.*)$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BulletExpression { get; }

    [GeneratedRegex(@"!\[[^\]]*\]\([^)]*\)", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ImageExpression { get; }

    [GeneratedRegex(@"\[(?<text>[^\]]*)\]\([^)]*\)", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex LinkExpression { get; }

    [GeneratedRegex(@"\s*\((?:part of )?(?:[\w.-]+/[\w.-]+)?#\d+(?:\s*,\s*#\d+)*\)", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IssueReferenceExpression { get; }

    [GeneratedRegex(@"\s+", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex WhitespaceExpression { get; }

    /// <summary>
    /// Splits a note into the text before its first <c>##</c> heading and its sections. Headings inside fenced code are
    /// text, not headings.
    /// </summary>
    /// <param name="markdown">The note.</param>
    /// <returns>The lead, and the sections in the order they are written.</returns>
    public static (string Lead, IReadOnlyList<ReleaseSection> Sections) Split(string markdown)
    {
        var lead = new List<string>();
        var sections = new List<(string Name, List<string> Lines)>();
        string? fence = null;
        foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var marker = FenceExpression.Match(line);
            if (marker.Success && (fence is null || (marker.Groups["marker"].Value[0] == fence[0] && marker.Groups["marker"].Value.Length >= fence.Length)))
            {
                fence = fence is null ? marker.Groups["marker"].Value : null;
            }

            var heading = fence is null && !marker.Success ? HeadingExpression.Match(line) : Match.Empty;
            if (heading.Success)
            {
                sections.Add((heading.Groups["name"].Value.Trim(), []));
            }
            else if (sections.Count == 0)
            {
                lead.Add(line);
            }
            else
            {
                sections[^1].Lines.Add(line);
            }
        }

        return (string.Join('\n', lead).Trim(), [.. sections.Select(section => new ReleaseSection(section.Name, string.Join('\n', section.Lines).Trim()))]);
    }

    /// <summary>
    /// Finds the one line that says what a release is about: the first paragraph written before the sections, or else the
    /// first thing a section lists.
    /// </summary>
    /// <param name="lead">The text before the first section.</param>
    /// <param name="sections">The sections of the note.</param>
    /// <returns>The line, without markdown or issue references; empty when the note says nothing.</returns>
    public static string Summarize(string lead, IReadOnlyList<ReleaseSection> sections)
    {
        foreach (var paragraph in lead.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var text = Plain(paragraph);
            if (text.Length > 0)
            {
                return text;
            }
        }

        foreach (var section in sections)
        {
            foreach (var line in section.Markdown.Split('\n'))
            {
                var item = BulletExpression.Match(line);
                var text = Plain(item.Success ? item.Groups["text"].Value : line);
                if (text.Length > 0)
                {
                    return text;
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Reads the record the capture writes beside a note.
    /// </summary>
    /// <param name="json">The record; <see langword="null"/> when there is none.</param>
    /// <returns>When the release was made and the pull request it was made from; each <see langword="null"/> when it is not recorded or the record cannot be read.</returns>
    public static (DateTimeOffset? ReleasedAt, string? Pull) ReadMetadata(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return (null, null);
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return (null, null);
            }

            DateTimeOffset? releasedAt = null;
            if (document.RootElement.TryGetProperty("releasedAt", out var at)
                && at.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(at.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
            {
                releasedAt = parsed;
            }

            string? pull = null;
            if (document.RootElement.TryGetProperty("pull", out var number))
            {
                pull = number.ValueKind switch
                {
                    JsonValueKind.Number => number.GetRawText(),
                    JsonValueKind.String => number.GetString(),
                    _ => null
                };
            }

            return (releasedAt, string.IsNullOrWhiteSpace(pull) ? null : pull);
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    static string Plain(string markdown)
    {
        var text = ImageExpression.Replace(markdown, string.Empty);
        text = LinkExpression.Replace(text, "${text}");
        text = IssueReferenceExpression.Replace(text, string.Empty);
        text = text.Replace("**", string.Empty).Replace("__", string.Empty).Replace("`", string.Empty).Replace("*", string.Empty);
        text = WhitespaceExpression.Replace(text, " ").Trim().TrimStart('#').Trim();
        return text.Length <= MaximumSummaryLength ? text : $"{text[..(MaximumSummaryLength - 1)].TrimEnd()}…";
    }
}
