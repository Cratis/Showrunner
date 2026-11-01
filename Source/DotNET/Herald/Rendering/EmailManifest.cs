// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.RegularExpressions;

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Represents the values an email kind accepts, declared in <c>{kind}.json</c> next to its templates.
/// </summary>
/// <remarks>
/// <code>
/// { "values": [ { "name": "invitation.link", "required": true }, { "name": "inviter.name" } ] }
/// </code>
/// A value is written in the template as <c>{{invitation.link}}</c>. A value that is not required and not given is empty.
/// </remarks>
/// <param name="Values">The declared values.</param>
public partial record EmailManifest(IReadOnlyList<EmailValueDeclaration> Values)
{
    /// <summary>
    /// The prefix of the values Herald provides to every template.
    /// </summary>
    public const string LookPrefix = "look.";

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_]*(?:\.[A-Za-z][A-Za-z0-9_]*)*$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ValueName { get; }

    /// <summary>
    /// Gets a value indicating whether a name can be used in a template: it is declared, or provided by Herald.
    /// </summary>
    /// <param name="name">The name of the value.</param>
    /// <returns>True when it can be used.</returns>
    public bool Accepts(string name) => name == "look.brandColor" || name == "look.footer" || Values.Any(value => value.Name == name);

    /// <summary>
    /// Reads a manifest.
    /// </summary>
    /// <param name="kind">The kind of email, for the messages.</param>
    /// <param name="json">The content of the manifest file.</param>
    /// <returns>The manifest.</returns>
    /// <exception cref="EmailCannotBeRendered">The manifest is not valid.</exception>
    public static EmailManifest Parse(EmailKind kind, string json)
    {
        List<EmailValueDeclaration> values = [];
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            if (document.RootElement.TryGetProperty("values", out var declared))
            {
                foreach (var element in declared.EnumerateArray())
                {
                    var name = element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetProperty("name").GetString();
                    var required = element.ValueKind == JsonValueKind.Object && element.TryGetProperty("required", out var flag) && flag.GetBoolean();
                    values.Add(new EmailValueDeclaration(name ?? string.Empty, required));
                }
            }
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw new EmailCannotBeRendered($"The values of the '{kind}' email can not be read: {exception.Message}");
        }

        foreach (var value in values)
        {
            if (!ValueName.IsMatch(value.Name) || value.Name.StartsWith(LookPrefix, StringComparison.Ordinal))
            {
                throw new EmailCannotBeRendered($"'{value.Name}' is not a value name the '{kind}' email can declare: use letters, digits and underscores in dotted segments, and not 'look'.");
            }
        }

        if (values.Select(value => value.Name).Distinct(StringComparer.Ordinal).Count() != values.Count)
        {
            throw new EmailCannotBeRendered($"The '{kind}' email declares a value twice.");
        }

        var conflict = values.FirstOrDefault(value => values.Exists(other => other.Name.StartsWith(value.Name + ".", StringComparison.Ordinal)));
        if (conflict is not null)
        {
            throw new EmailCannotBeRendered($"The '{kind}' email declares '{conflict.Name}' both as a value and as a group of values.");
        }

        return new(values);
    }
}
