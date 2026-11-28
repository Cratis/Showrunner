// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Reporting.Attachments;

/// <summary>
/// The transport boundary a report dialog's paste/drop-to-upload reaches, and the static file serving of what it
/// uploaded. Deliberately outside Arc's command pipeline - an uploaded file is not a domain fact anything reduces
/// from, just a file a report's description or details ends up linking to.
/// </summary>
public static class UsherAttachmentEndpoints
{
    /// <summary>
    /// The header the original file name travels in, so the stored file keeps its extension.
    /// </summary>
    public const string FileNameHeader = "X-File-Name";

    /// <summary>
    /// The route the attachment endpoint and its static file serving are mapped at. A literal route rather than one
    /// under the <c>/api/usher/...</c> namespace Arc reflects Usher's own commands onto, so a host that rewrites
    /// Usher's command routes (as Cratis Direct does) never collides with it.
    /// </summary>
    public const string Route = "/usher-attachments";

    /// <summary>
    /// The types served inline. Everything else, including SVG, is served as a download: a short, deliberately
    /// narrow allow-list rather than "anything that looks like an image" - a browser rendering an uploaded file
    /// inline runs it in this application's own origin, so an HTML or SVG file would be a way to run script against
    /// the session of whoever opens it.
    /// </summary>
    static readonly Dictionary<string, string> _inlineContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
    };

    /// <summary>
    /// Maps the upload endpoint. Requires an authenticated caller, the same bar as every Usher command.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to map on.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    public static IEndpointRouteBuilder MapUsherAttachments(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(Route, async (HttpContext context, IOptions<UsherOptions> options, CancellationToken cancellationToken) =>
        {
            var configured = options.Value.Attachments;
            if (context.Request.ContentLength is null or 0 || context.Request.ContentLength > configured.MaxSizeInBytes)
            {
                return Results.BadRequest($"An attachment must be between 1 byte and {configured.MaxSizeInBytes:N0} bytes.");
            }

            var extension = ExtensionFor(context.Request.Headers[FileNameHeader].ToString(), context.Request.ContentType?.Split(';')[0].Trim() ?? string.Empty);

            Directory.CreateDirectory(configured.Path);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var path = Path.Combine(configured.Path, fileName);

            await using var file = File.Create(path);
            await context.Request.Body.CopyToAsync(file, cancellationToken);

            return Results.Ok(new
            {
                url = $"{context.Request.Scheme}://{context.Request.Host}{Route}/{fileName}",
                isImage = IsAllowListedInlineExtension(extension),
            });
        }).RequireAuthorization();

        return endpoints;
    }

    /// <summary>
    /// Serves uploaded attachments from the configured directory at <see cref="Route"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> for chaining.</returns>
    public static WebApplication UseUsherAttachments(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<UsherOptions>>().Value.Attachments;
        Directory.CreateDirectory(options.Path);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(options.Path),
            RequestPath = Route,

            // Without this an attachment whose extension the static-file middleware does not know is a 404, which
            // presents as a link that silently goes nowhere.
            ServeUnknownFileTypes = true,
            DefaultContentType = "application/octet-stream",
            OnPrepareResponse = PrepareAttachmentResponse,
        });

        return app;
    }

    /// <summary>
    /// Decides how a stored attachment is handed to the browser. Every response forbids content sniffing, so a file
    /// is only ever treated as the type it is served as.
    /// </summary>
    /// <param name="served">The file about to be served.</param>
    public static void PrepareAttachmentResponse(StaticFileResponseContext served)
    {
        served.Context.Response.Headers.XContentTypeOptions = "nosniff";

        var extension = Path.GetExtension(served.File.Name);
        if (_inlineContentTypes.TryGetValue(extension, out var contentType))
        {
            served.Context.Response.ContentType = contentType;
            return;
        }

        // Anything else, including SVG, is handed over as a download rather than rendered, so an uploaded page,
        // script or vector image can never run in this application's origin.
        served.Context.Response.ContentType = "application/octet-stream";
        served.Context.Response.Headers.ContentDisposition = "attachment";
    }

    /// <summary>
    /// Whether an extension is one of the types attachments render inline rather than as a download.
    /// </summary>
    /// <param name="extension">The extension to check, including the leading dot.</param>
    /// <returns><see langword="true"/> if the extension renders inline; otherwise, <see langword="false"/>.</returns>
    public static bool IsAllowListedInlineExtension(string extension) => _inlineContentTypes.ContainsKey(extension);

    /// <summary>
    /// The extension a stored attachment keeps.
    /// </summary>
    /// <remarks>
    /// The original name's own extension where there is one, because that is what makes an attachment open in the
    /// right thing when somebody downloads it. Only the extension is kept - the stored name is a fresh identifier,
    /// so nothing a caller sends can reach out of the directory or collide with what is already in it.
    /// </remarks>
    /// <param name="fileName">The original file name, when the caller sent one.</param>
    /// <param name="contentType">The declared content type, used when the name carries no extension.</param>
    /// <returns>The extension, including the leading dot, or empty when neither says.</returns>
    public static string ExtensionFor(string fileName, string contentType)
    {
        var fromName = Path.GetExtension(fileName.AsSpan()).ToString();
        if (fromName.Length > 1 && fromName[1..].All(char.IsLetterOrDigit))
        {
            return fromName.ToLowerInvariant();
        }

        return contentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => string.Empty,
        };
    }
}
