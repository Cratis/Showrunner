// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Finds the templates in the templates folder.
/// </summary>
/// <remarks>
/// A template is <c>{kind}.{language}.html</c>. It starts with a header giving the subject and, optionally, the preview
/// text, between two lines of three dashes:
/// <code>
/// ---
/// subject: You are invited to {{product.name}}
/// preview: {{inviter.name}} invited you
/// ---
/// &lt;html&gt;…&lt;/html&gt;
/// </code>
/// Templates are read when an email is rendered, so editing a file takes effect on the next email.
/// </remarks>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
public partial class TemplateStore(IOptions<HeraldOptions> options)
{
    [GeneratedRegex(@"^[A-Za-z]{2,3}(?:-[A-Za-z0-9]{2,8})*$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex LanguageName { get; }

    [GeneratedRegex(@"^[a-z0-9][a-z0-9\-]*$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex KindName { get; }

    /// <summary>
    /// Finds the template of an email kind in a language, or in the closest language there is: the language without its
    /// region, then the default language.
    /// </summary>
    /// <param name="kind">The kind of email.</param>
    /// <param name="language">The language asked for; null for the default language.</param>
    /// <returns>The template.</returns>
    /// <exception cref="EmailCannotBeRendered">There is no such email, or its template does not follow the rules.</exception>
    public EmailTemplate Find(EmailKind kind, EmailLanguage? language)
    {
        if (!KindName.IsMatch(kind.Value))
        {
            throw new EmailCannotBeRendered($"'{kind}' is not an email kind: use lowercase letters, digits and dashes.");
        }

        var folder = options.Value.TemplatesFolder;
        var manifestPath = System.IO.Path.Combine(folder, $"{kind}.json");
        if (!File.Exists(manifestPath))
        {
            throw new EmailCannotBeRendered($"There is no email '{kind}': {kind}.json, which declares its values, is not in the templates folder.");
        }

        foreach (var candidate in Candidates(language))
        {
            var path = System.IO.Path.Combine(folder, $"{kind}.{candidate}.html");
            if (File.Exists(path))
            {
                return Read(kind, candidate, EmailManifest.Parse(kind, File.ReadAllText(manifestPath)), File.ReadAllText(path));
            }
        }

        throw new EmailCannotBeRendered($"The '{kind}' email has no template in '{language?.Value ?? options.Value.DefaultLanguage.Value}' or in the default language '{options.Value.DefaultLanguage}'.");
    }

    static EmailTemplate Read(EmailKind kind, EmailLanguage language, EmailManifest manifest, string content)
    {
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        if (lines.Length < 3 || lines[0].Trim() != "---")
        {
            throw new EmailCannotBeRendered($"The '{kind}' template in '{language}' must start with a header: a line of three dashes, 'subject: …', and a line of three dashes.");
        }

        var end = Array.FindIndex(lines, 1, line => line.Trim() == "---");
        if (end < 0)
        {
            throw new EmailCannotBeRendered($"The header of the '{kind}' template in '{language}' is never closed with a line of three dashes.");
        }

        string subject = string.Empty, preview = string.Empty;
        foreach (var line in lines[1..end].Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            var key = separator < 0 ? line.Trim() : line[..separator].Trim();
            var value = separator < 0 ? string.Empty : line[(separator + 1)..].Trim();
            switch (key)
            {
                case "subject":
                    subject = value;
                    break;
                case "preview":
                    preview = value;
                    break;
                default:
                    throw new EmailCannotBeRendered($"The header of the '{kind}' template in '{language}' has '{key}', which is not 'subject' or 'preview'.");
            }
        }

        if (subject.Length == 0)
        {
            throw new EmailCannotBeRendered($"The '{kind}' template in '{language}' has no subject.");
        }

        var body = string.Join('\n', lines[(end + 1)..]);
        TemplateTags.Check(kind, "subject", subject, manifest);
        TemplateTags.Check(kind, "preview text", preview, manifest);
        TemplateTags.Check(kind, "body", body, manifest);
        return new(kind, language, manifest, subject, preview, body);
    }

    IEnumerable<EmailLanguage> Candidates(EmailLanguage? language)
    {
        var asked = language is { Value.Length: > 0 } ? language.Value : options.Value.DefaultLanguage.Value;
        if (!LanguageName.IsMatch(asked) || !LanguageName.IsMatch(options.Value.DefaultLanguage.Value))
        {
            throw new EmailCannotBeRendered($"'{asked}' is not a language: use a culture name such as 'en' or 'nb-NO'.");
        }

        var names = new List<string> { asked };
        if (asked.Contains('-', StringComparison.Ordinal))
        {
            names.Add(asked[..asked.IndexOf('-', StringComparison.Ordinal)]);
        }

        names.Add(options.Value.DefaultLanguage.Value);
        return names.Distinct(StringComparer.Ordinal).Select(name => new EmailLanguage(name));
    }
}
