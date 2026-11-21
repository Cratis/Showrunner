// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// Makes error text safe to put in a finding: custom resource errors and transport errors can contain connection strings,
/// authorization headers and credentials, and a finding is delivered to chat and email.
/// </summary>
public static partial class Redaction
{
    const int MaximumLength = 300;
    const int RegexTimeoutMilliseconds = 1000;

    [GeneratedRegex(@"\b[a-z][a-z0-9+.-]*://\S+", RegexOptions.IgnoreCase, RegexTimeoutMilliseconds)]
    private static partial Regex Url { get; }

    [GeneratedRegex(@"\b(?:bearer|basic)\s+\S+", RegexOptions.IgnoreCase, RegexTimeoutMilliseconds)]
    private static partial Regex Authorization { get; }

    [GeneratedRegex(@"\b(?:password|passwd|pwd|token|secret|access[_-]?key|secret[_-]?key)\b[""']?\s*[:=]\s*[""']?[^,\s;""']+", RegexOptions.IgnoreCase, RegexTimeoutMilliseconds)]
    private static partial Regex Credential { get; }

    /// <summary>
    /// Redacts URLs, authorization values and credential assignments, collapses whitespace and truncates.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>A single line of at most 300 characters that carries no URL or credential.</returns>
    public static string Summarize(string? text)
    {
        var result = Url.Replace(text ?? string.Empty, "[redacted URL]");
        result = Authorization.Replace(result, "[redacted authorization]");
        result = Credential.Replace(result, "[redacted credential]");
        result = string.Join(' ', result.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return result.Length <= MaximumLength ? result : result[..MaximumLength];
    }
}
