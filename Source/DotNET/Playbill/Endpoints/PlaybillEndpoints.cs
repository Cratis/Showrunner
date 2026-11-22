// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Cratis.Showrunner.Playbill.Endpoints;

/// <summary>
/// The handlers of the endpoints under <see cref="PlaybillPaths.Group"/>.
/// </summary>
public static class PlaybillEndpoints
{
    const string MarkdownContentType = "text/markdown; charset=utf-8";

    /// <summary>
    /// Lists the releases, the newest first.
    /// </summary>
    /// <param name="notes">The release history.</param>
    /// <param name="since">When set, only the releases newer than this version.</param>
    /// <returns>The list.</returns>
    public static IResult List(IReleaseNotes notes, string? since)
    {
        var releases = notes.All.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(since))
        {
            ReleaseVersion floor = since;
            if (!floor.IsValid)
            {
                return Results.Problem($"'{since}' is not a version.", statusCode: StatusCodes.Status400BadRequest);
            }

            releases = releases.Where(note => note.Version > floor);
        }

        return Results.Json(new ReleaseListResponse(CurrentOrNull(notes), [.. releases.Select(ReleaseSummaryResponse.From)]));
    }

    /// <summary>
    /// Gives the note of a release: its markdown, or the parsed note when the caller asks for JSON.
    /// </summary>
    /// <param name="notes">The release history.</param>
    /// <param name="version">The version of the release.</param>
    /// <param name="http">The request.</param>
    /// <returns>The note.</returns>
    public static IResult Get(IReleaseNotes notes, string version, HttpContext http)
    {
        var note = notes.Find(version);
        if (note is null)
        {
            return Results.NotFound();
        }

        // The same address answers both representations, so a cache must tell them apart.
        http.Response.Headers.Vary = HeaderNames.Accept;
        var wantsJson = http.Request.GetTypedHeaders().Accept.Any(accept => accept.MediaType.Value?.Contains("json", StringComparison.OrdinalIgnoreCase) == true);
        return wantsJson
            ? Results.Json(ReleaseResponse.From(note))
            : Results.Text(note.Markdown, MarkdownContentType);
    }

    /// <summary>
    /// Gives a file a release note refers to. The release history never changes while the application runs, so the file can
    /// be cached for as long as the build lives.
    /// </summary>
    /// <param name="notes">The release history.</param>
    /// <param name="version">The version of the release.</param>
    /// <param name="asset">The name of the file inside the release's folder.</param>
    /// <returns>The file.</returns>
    public static IResult GetAsset(IReleaseNotes notes, string version, string asset)
    {
        var file = notes.FindAsset(version, asset);
        if (file is null)
        {
            return Results.NotFound();
        }

        var tag = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes($"{notes.Fingerprint}/{version}/{asset}")))[..32];
        return new CachedFileResult(file, new EntityTagHeaderValue($"\"{tag}\""));
    }

    /// <summary>
    /// Gives the version the application is running and the newest release that has a note.
    /// </summary>
    /// <param name="notes">The release history.</param>
    /// <returns>The current release.</returns>
    public static IResult GetCurrent(IReleaseNotes notes)
    {
        var current = notes.Current.IsSet ? notes.Find(notes.Current) : null;
        return Results.Json(new CurrentReleaseResponse(
            CurrentOrNull(notes),
            current is null ? null : ReleaseSummaryResponse.From(current),
            notes.All.Count > 0 ? notes.All[0].Version.Value : null));
    }

    static string? CurrentOrNull(IReleaseNotes notes) => notes.Current.IsSet ? notes.Current.Value : null;
}
