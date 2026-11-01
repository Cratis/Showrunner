// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Cratis.Showrunner.Notes.Collecting.Transport;

/// <summary>
/// Handles the three calls of the HTTP transport: authenticates the bearer secret through the host's
/// <see cref="IResolveNotesPublications"/> and answers what <see cref="NoteCollector"/> decides.
/// </summary>
/// <remarks>
/// A missing, malformed or wrong secret and an unknown publication all answer <c>401</c>, the same, so a caller learns
/// nothing about which publications exist.
/// </remarks>
/// <param name="publications">The host's authentication of publications.</param>
/// <param name="collector">Stores the notes and answers.</param>
public class NoteCollectingCalls(IResolveNotesPublications publications, NoteCollector collector)
{
    /// <summary>
    /// The most notes accepted in one request.
    /// </summary>
    public const int MostNotesPerRequest = 1000;

    const string BearerPrefix = "Bearer ";

    /// <summary>
    /// Stores the notes posted.
    /// </summary>
    /// <param name="publicationId">The publication in the route.</param>
    /// <param name="http">The request.</param>
    /// <returns>The result.</returns>
    public async Task<IResult> Receive(string publicationId, HttpContext http)
    {
        if (await Authenticate(publicationId, http) is not var (id, publication))
        {
            return Unauthorized(http);
        }

        PublishedNoteEvent[]? notes;
        try
        {
            notes = await JsonSerializer.DeserializeAsync<PublishedNoteEvent[]>(http.Request.Body, NotesTransportJson.Options, http.RequestAborted);
        }
        catch (JsonException)
        {
            return Results.BadRequest();
        }

        if (notes is null)
        {
            return Results.BadRequest();
        }

        if (notes.Length > MostNotesPerRequest)
        {
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        var source = Source(http);
        var outcome = await collector.Collect(id, publication, source, notes);
        if (outcome != CollectOutcome.Stored)
        {
            return Rejected(outcome);
        }

        var (_, position) = await collector.Position(id, publication, source);
        return Results.Json(new NotesPosition(position), NotesTransportJson.Options);
    }

    /// <summary>
    /// Answers the position of the notes stored.
    /// </summary>
    /// <param name="publicationId">The publication in the route.</param>
    /// <param name="http">The request.</param>
    /// <returns>The result.</returns>
    public async Task<IResult> Position(string publicationId, HttpContext http)
    {
        if (await Authenticate(publicationId, http) is not var (id, publication))
        {
            return Unauthorized(http);
        }

        var (outcome, position) = await collector.Position(id, publication, Source(http));
        return outcome == CollectOutcome.Stored ? Results.Json(new NotesPosition(position), NotesTransportJson.Options) : Rejected(outcome);
    }

    /// <summary>
    /// Answers what was done with the notes of the publication after a cursor.
    /// </summary>
    /// <param name="publicationId">The publication in the route.</param>
    /// <param name="http">The request.</param>
    /// <returns>The result.</returns>
    public async Task<IResult> Responses(string publicationId, HttpContext http)
    {
        if (await Authenticate(publicationId, http) is not var (id, publication))
        {
            return Unauthorized(http);
        }

        var after = ulong.TryParse(http.Request.Query[NotesTransportPaths.AfterParameter], out var cursor) ? cursor : 0;
        var (outcome, responses) = await collector.Responses(id, publication, after);
        return outcome == CollectOutcome.Stored ? Results.Json(responses, NotesTransportJson.Options) : Rejected(outcome);
    }

    static string Source(HttpContext http) => http.Request.Query[NotesTransportPaths.SourceParameter].ToString();

    static IResult Rejected(CollectOutcome outcome) => outcome switch
    {
        CollectOutcome.TenantMismatch => Results.StatusCode(StatusCodes.Status409Conflict),
        CollectOutcome.Invalid => Results.BadRequest(),
        _ => Results.StatusCode(StatusCodes.Status422UnprocessableEntity)
    };

    static IResult Unauthorized(HttpContext http)
    {
        http.Response.Headers[HeaderNames.WWWAuthenticate] = "Bearer";
        return Results.StatusCode(StatusCodes.Status401Unauthorized);
    }

    async Task<(PublicationId Id, NotesPublication Publication)?> Authenticate(string publicationId, HttpContext http)
    {
        var header = http.Request.Headers.Authorization.ToString();
        if (publicationId.Length == 0 || !header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = header[BearerPrefix.Length..].Trim();
        if (token.Length == 0)
        {
            return null;
        }

        var id = new PublicationId(publicationId);
        return await publications.Resolve(id, token) is { } publication ? (id, publication) : null;
    }
}
