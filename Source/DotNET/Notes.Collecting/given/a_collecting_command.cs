// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Notes.Collecting.given;

/// <summary>
/// A command on a received note, run through the command pipeline by a signed-in person on the collecting side.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public class a_collecting_command<TCommand> : Specification
    where TCommand : class
{
    protected CommandScenario<TCommand> _scenario;
    protected NoteId _noteId;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as; a signed-in person by default.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Operators.SignedIn();

    void Establish()
    {
        _noteId = NoteId.New();
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddNoteCollecting(_ => { });
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on the note before the command runs.
    /// </summary>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(_noteId).Events(events);
}
#endif
