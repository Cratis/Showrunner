// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Integration.given;

/// <summary>
/// Template and image files for an invitation email, in a folder of their own.
/// </summary>
public sealed class AnInvitationTemplate : IDisposable
{
    const string Manifest = """{ "values": [ { "name": "invitation.link", "required": true }, "inviter.name", "inviter.message" ] }""";

    const string English = "---\nsubject: {{inviter.name}} invited you to Ledger\npreview: Accept the invitation\n---\n" +
        "<html><head><style>.button { background-color: {{look.brandColor}}; color: #ffffff; }</style></head><body>" +
        "<img src=\"cid:logo.png\" alt=\"Ledger\"><p>{{inviter.name}} invited you.</p>" +
        "{{#if inviter.message}}<blockquote>{{inviter.message}}</blockquote>{{/if}}" +
        "<a class=\"button\" href=\"{{invitation.link}}\">Accept</a><p>{{look.footer}}</p></body></html>";

    static readonly byte[] _png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    readonly string _root = Directory.CreateTempSubdirectory("herald-integration-").FullName;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnInvitationTemplate"/> class.
    /// </summary>
    public AnInvitationTemplate()
    {
        Templates = Path.Combine(_root, "templates");
        Images = Path.Combine(_root, "images");
        Directory.CreateDirectory(Templates);
        Directory.CreateDirectory(Images);
        File.WriteAllText(Path.Combine(Templates, "invitation.json"), Manifest);
        File.WriteAllText(Path.Combine(Templates, "invitation.en.html"), English);
        File.WriteAllBytes(Path.Combine(Images, "logo.png"), _png);
    }

    /// <summary>
    /// Gets the folder with the templates.
    /// </summary>
    public string Templates { get; }

    /// <summary>
    /// Gets the folder with the images.
    /// </summary>
    public string Images { get; }

    /// <inheritdoc/>
    public void Dispose() => Directory.Delete(_root, recursive: true);
}
