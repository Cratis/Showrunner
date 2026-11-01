// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Checks that the placeholders of a template stay within what Herald supports: values the email declares, <c>#if</c>
/// and <c>date</c>. Nothing else is accepted — no unescaped output, partials or other helpers — so a template can not
/// bypass the encoding of values.
/// </summary>
public static partial class TemplateTags
{
    [GeneratedRegex(@"\{\{(?<content>.*?)\}\}", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Tag { get; }

    [GeneratedRegex(@"^date\s+(?<name>[A-Za-z][A-Za-z0-9_.]*)\s+""[^""{}]*""$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DateTag { get; }

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_]*(?:\.[A-Za-z][A-Za-z0-9_]*)*$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Path { get; }

    /// <summary>
    /// Checks all placeholders in a piece of a template.
    /// </summary>
    /// <param name="kind">The kind of email, for the messages.</param>
    /// <param name="part">What is being checked: <c>subject</c>, <c>preview text</c> or <c>body</c>.</param>
    /// <param name="text">The text.</param>
    /// <param name="manifest">The values the email accepts.</param>
    /// <exception cref="EmailCannotBeRendered">A placeholder is not supported or names an unknown value.</exception>
    public static void Check(EmailKind kind, string part, string text, EmailManifest manifest)
    {
        foreach (Match match in Tag.Matches(text))
        {
            var content = match.Groups["content"].Value.Trim();
            if (content.StartsWith('!'))
            {
                continue;
            }

            if (content == "/if" || content == "else")
            {
                continue;
            }

            if (content.StartsWith("#if ", StringComparison.Ordinal))
            {
                RequireKnown(kind, part, content[4..].Trim(), manifest);
                continue;
            }

            var date = DateTag.Match(content);
            if (date.Success)
            {
                RequireKnown(kind, part, date.Groups["name"].Value, manifest);
                continue;
            }

            if (Path.IsMatch(content))
            {
                RequireKnown(kind, part, content, manifest);
                continue;
            }

            throw new EmailCannotBeRendered($"The {part} of the '{kind}' email uses '{{{{{content}}}}}', which Herald does not support: use a value, {{{{#if value}}}} or {{{{date value \"format\"}}}}.");
        }
    }

    static void RequireKnown(EmailKind kind, string part, string name, EmailManifest manifest)
    {
        if (!Path.IsMatch(name) || !manifest.Accepts(name))
        {
            throw new EmailCannotBeRendered($"The {part} of the '{kind}' email uses '{{{{{name}}}}}', which is not a value of that email.");
        }
    }
}
