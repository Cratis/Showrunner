// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.given;

/// <summary>
/// A folder holding an invitation email in English and Norwegian, an images folder with a PNG and an SVG, and the
/// renderer over them.
/// </summary>
public class a_templates_folder : Specification
{
    protected const string Manifest = """
        { "values": [
            { "name": "invitation.link", "required": true },
            "invitation.expiresAt",
            "inviter.name",
            "inviter.message",
            "product.name"
        ] }
        """;

    protected const string English = """
        ---
        subject: {{inviter.name}} invited you to {{product.name}}
        preview: Accept the invitation to {{product.name}}
        ---
        <html><head><style>.button { background-color: {{look.brandColor}}; color: #ffffff; } p { font-family: Arial; }</style></head>
        <body><table><tr><td>
        <img src="cid:logo.png" alt="Ledger">
        <p>Hello, {{inviter.name}} invited you.</p>
        {{#if inviter.message}}<blockquote>{{inviter.message}}</blockquote>{{/if}}
        <p>Expires {{date invitation.expiresAt "d MMMM"}}</p>
        <a class="button" href="{{invitation.link}}">Accept</a>
        </td></tr></table>
        <p>{{look.footer}}</p></body></html>
        """;

    protected const string Norwegian = """
        ---
        subject: {{inviter.name}} har invitert deg til {{product.name}}
        ---
        <html><body><p>Utløper {{date invitation.expiresAt "d. MMMM"}}</p><a href="{{invitation.link}}">Godta</a></body></html>
        """;

    protected static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    protected string _folder;
    protected string _templates;
    protected string _images;
    protected HeraldOptions _options;
    protected EmailRenderer _renderer;

    protected static IReadOnlyDictionary<string, string> Values(params (string Name, string Value)[] values) =>
        values.ToDictionary(value => value.Name, value => value.Value);

    protected static IReadOnlyDictionary<string, string> InvitationValues => Values(
        ("invitation.link", "https://ledger.example.com/join?token=abc&x=1"),
        ("inviter.name", "Ada"),
        ("product.name", "Ledger"),
        ("invitation.expiresAt", "2026-11-15T10:00:00Z"));

    protected void Write(string relative, string content) => File.WriteAllText(System.IO.Path.Combine(_templates, relative), content);

    void Establish()
    {
        _folder = Directory.CreateTempSubdirectory("herald-").FullName;
        _templates = System.IO.Path.Combine(_folder, "templates");
        _images = System.IO.Path.Combine(_folder, "images");
        Directory.CreateDirectory(_templates);
        Directory.CreateDirectory(_images);
        File.WriteAllBytes(System.IO.Path.Combine(_images, "logo.png"), Png);
        File.WriteAllText(System.IO.Path.Combine(_images, "logo.svg"), "<svg xmlns=\"http://www.w3.org/2000/svg\"/>");

        Write("invitation.json", Manifest);
        Write("invitation.en.html", English);
        Write("invitation.nb.html", Norwegian);

        _options = new HeraldOptions
        {
            TemplatesFolder = _templates,
            ImagesFolder = _images,
            Sender = new() { Address = "noreply@ledger.example.com", Name = "Ledger" },
            Look = new() { BrandColor = "#0b5fff", Footer = "Ledger AS, Oslo" },
            Transport = EmailTransportKind.Folder,
            PickupFolder = System.IO.Path.Combine(_folder, "outbox")
        };

        var wrapped = Options.Create(_options);
        _renderer = new(new TemplateStore(wrapped), new EmailImages(wrapped), wrapped);
    }

    void Destroy() => Directory.Delete(_folder, recursive: true);
}
#endif
