// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Notes.given;

/// <summary>
/// A command on a note, run through the command pipeline by a signed-in giver in an application with Notes switched on.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public class a_note_command<TCommand> : Specification
    where TCommand : class
{
    protected CommandScenario<TCommand> _scenario;
    protected NoteId _noteId;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as; the giver by default.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Givers.SignedInAs(Givers.UserId);

    /// <summary>
    /// Gets whether Notes is switched on; it is by default.
    /// </summary>
    protected virtual bool Enabled => true;

    void Establish()
    {
        _noteId = NoteId.New();
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddNotes(notes =>
        {
            notes.Enabled = Enabled;
            notes.ApplicationName = "Ledger";
            notes.PublicationId = "pub-7f3a";
            notes.ApplicationVersion = "1.2.3";
        });
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on the note before the command runs.
    /// </summary>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(_noteId).Events(events);
}
#endif
