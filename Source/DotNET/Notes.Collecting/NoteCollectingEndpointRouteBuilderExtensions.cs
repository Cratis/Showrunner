// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Transport;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for mapping the endpoints applications that run their own Chronicle kernel relay their notes to.
/// </summary>
public static class NoteCollectingEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the HTTP transport of Notes under <c>/.cratis/notes/v1/publications/{publicationId}</c>: <c>POST notes</c>,
    /// <c>GET notes/position</c> and <c>GET responses</c>.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for continuation.</returns>
    /// <exception cref="InvalidOperationException">The host did not register an <see cref="IResolveNotesPublications"/> or call <c>AddNoteCollecting</c>.</exception>
    /// <remarks>
    /// <para>
    /// The endpoints allow anonymous callers: they do not use the host's user authentication. A caller is a published
    /// application, authenticated by the bearer secret of its publication, which the host's
    /// <see cref="IResolveNotesPublications"/> checks.
    /// </para>
    /// <para>
    /// Expose them only on an address the published applications reach and the public does not: an internal listener or
    /// service, not the host's public ingress.
    /// </para>
    /// </remarks>
    public static IEndpointRouteBuilder MapNoteCollecting(this IEndpointRouteBuilder endpoints)
    {
        if (endpoints.ServiceProvider.GetService<IResolveNotesPublications>() is null || endpoints.ServiceProvider.GetService<NoteCollectingCalls>() is null)
        {
            throw new InvalidOperationException("MapNoteCollecting needs AddNoteCollecting and an IResolveNotesPublications that authenticates the publications; register one in the service collection.");
        }

        var group = endpoints.MapGroup(NotesTransportPaths.Group).AllowAnonymous().ExcludeFromDescription();
        group.MapPost(NotesTransportPaths.Notes, (string publicationId, HttpContext http, NoteCollectingCalls calls) => calls.Receive(publicationId, http));
        group.MapGet(NotesTransportPaths.Position, (string publicationId, HttpContext http, NoteCollectingCalls calls) => calls.Position(publicationId, http));
        group.MapGet(NotesTransportPaths.Responses, (string publicationId, HttpContext http, NoteCollectingCalls calls) => calls.Responses(publicationId, http));
        return endpoints;
    }
}
