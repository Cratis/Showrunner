// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Notes.given;

/// <summary>
/// A query run through the query pipeline by a signed-in giver.
/// </summary>
/// <typeparam name="TReadModel">The read model the query is on.</typeparam>
public class a_query<TReadModel> : Specification
{
    protected QueryScenario<TReadModel> _scenario;
    protected QueryResult _result;

    /// <summary>
    /// Gets the principal the query runs as; the giver by default.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Givers.SignedInAs(Givers.UserId);

    /// <summary>
    /// Gets whether Notes is switched on; it is by default.
    /// </summary>
    protected virtual bool Enabled => true;

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddNotes(notes =>
        {
            notes.Enabled = Enabled;
            notes.ApplicationName = "Ledger";
            notes.PublicationId = "pub-7f3a";
        });
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on a note.
    /// </summary>
    /// <param name="noteId">The note.</param>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(NoteId noteId, params object[] events) => _scenario.Given.ForEventSource(noteId).Events(events);
}
#endif
