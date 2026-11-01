// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Renders an email: fills its template with values in strict mode, moves its styles inline, adds the preview text,
/// finds its images and generates the plain-text alternative.
/// </summary>
/// <remarks>
/// <para>
/// Strict means an unknown value is an error, never an empty string, and a required value that is missing or blank is an
/// error. Every value is HTML-encoded in the body, so a person's message can not inject markup; the subject and preview
/// text are plain text and are not encoded. The only constructs are values, <c>#if</c> and <c>date</c>.
/// </para>
/// <para>
/// A rendered email has no placeholders left: either every one is filled or rendering fails with
/// <see cref="EmailCannotBeRendered"/> and nothing is sent.
/// </para>
/// </remarks>
/// <param name="templates">The <see cref="TemplateStore"/>.</param>
/// <param name="images">The <see cref="EmailImages"/>.</param>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
public partial class EmailRenderer(TemplateStore templates, EmailImages images, IOptions<HeraldOptions> options)
{
    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Whitespace { get; }

    [GeneratedRegex(@"<body\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BodyTag { get; }

    /// <summary>
    /// Renders an email.
    /// </summary>
    /// <param name="kind">The kind of email, which is its template.</param>
    /// <param name="language">The language asked for; null for the default language.</param>
    /// <param name="values">The values, by the names the email declares.</param>
    /// <returns>The <see cref="RenderedEmail"/>.</returns>
    /// <exception cref="EmailCannotBeRendered">The email can not be rendered.</exception>
    public RenderedEmail Render(EmailKind kind, EmailLanguage? language, IReadOnlyDictionary<string, string> values)
    {
        var template = templates.Find(kind, language);
        var data = DataFor(template, values);
        var culture = CultureFor(template.Language);

        var subject = Whitespace.Replace(Fill(template, template.Subject, data, culture, encode: false), " ").Trim();
        var preview = Whitespace.Replace(Fill(template, template.PreviewText, data, culture, encode: false), " ").Trim();
        var filled = Fill(template, template.Body, data, culture, encode: true);

        var inlined = PreMailer.Net.PreMailer.MoveCssInline(filled, removeStyleElements: false, preserveMediaQueries: true).Html;
        var text = PlainText.From(inlined);
        var html = preview.Length == 0 ? inlined : WithPreview(inlined, preview);
        return new(kind, template.Language, subject, html, text, images.ReferencedBy(html));
    }

    static string WithPreview(string html, string preview)
    {
        var hidden = $"<div style=\"display:none;max-height:0;overflow:hidden;opacity:0;mso-hide:all\">{WebUtility.HtmlEncode(preview)}</div>";
        var body = BodyTag.Match(html);
        return body.Success ? html.Insert(body.Index + body.Length, hidden) : hidden + html;
    }

    static CultureInfo CultureFor(EmailLanguage language)
    {
        try
        {
            return CultureInfo.GetCultureInfo(language.Value);
        }
        catch (CultureNotFoundException)
        {
            throw new EmailCannotBeRendered($"'{language}' is not a culture this system knows, so dates can not be formatted in it.");
        }
    }

    static void RegisterDate(IHandlebars handlebars, CultureInfo culture) =>
        handlebars.RegisterHelper("date", (writer, _, arguments) =>
        {
            var text = arguments[0]?.ToString() ?? string.Empty;
            if (text.Length == 0)
            {
                return;
            }

            if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
            {
                throw new EmailCannotBeRendered("A value used with 'date' is not a date: give it as ISO 8601, such as 2026-11-15 or 2026-11-15T10:30:00Z.");
            }

            string formatted;
            try
            {
                formatted = date.ToString(arguments[1]?.ToString() ?? "d", culture);
            }
            catch (FormatException)
            {
                throw new EmailCannotBeRendered($"'{arguments[1]}' is not a date format.");
            }

            writer.Write(formatted);
        });

    static void Set(Dictionary<string, object> data, string name, string value)
    {
        var segments = name.Split('.');
        var current = data;
        foreach (var segment in segments[..^1])
        {
            if (!current.TryGetValue(segment, out var child))
            {
                child = new Dictionary<string, object>();
                current[segment] = child;
            }

            current = (Dictionary<string, object>)child;
        }

        current[segments[^1]] = value;
    }

    static string Fill(EmailTemplate template, string text, Dictionary<string, object> data, CultureInfo culture, bool encode)
    {
        if (text.Length == 0)
        {
            return text;
        }

        var handlebars = Handlebars.Create(new HandlebarsConfiguration
        {
            ThrowOnUnresolvedBindingExpression = true,
            NoEscape = !encode
        });
        RegisterDate(handlebars, culture);
        try
        {
            return handlebars.Compile(text)(data);
        }
        catch (EmailCannotBeRendered)
        {
            throw;
        }
        catch (HandlebarsException exception)
        {
            throw new EmailCannotBeRendered($"The '{template.Kind}' template in '{template.Language}' can not be rendered: {exception.InnerException?.Message ?? exception.Message}");
        }
    }

    Dictionary<string, object> DataFor(EmailTemplate template, IReadOnlyDictionary<string, string> values)
    {
        var unknown = values.Keys.FirstOrDefault(name => template.Manifest.Values.All(declared => declared.Name != name));
        if (unknown is not null)
        {
            throw new EmailCannotBeRendered($"'{unknown}' is not a value of the '{template.Kind}' email.");
        }

        var missing = template.Manifest.Values.FirstOrDefault(declared => declared.Required && (!values.TryGetValue(declared.Name, out var value) || string.IsNullOrWhiteSpace(value)));
        if (missing is not null)
        {
            throw new EmailCannotBeRendered($"The '{template.Kind}' email needs '{missing.Name}', and it was not given.");
        }

        Dictionary<string, object> data = [];
        foreach (var declared in template.Manifest.Values)
        {
            Set(data, declared.Name, values.GetValueOrDefault(declared.Name) ?? string.Empty);
        }

        Set(data, "look.brandColor", options.Value.Look.BrandColor);
        Set(data, "look.footer", options.Value.Look.Footer);
        return data;
    }
}
