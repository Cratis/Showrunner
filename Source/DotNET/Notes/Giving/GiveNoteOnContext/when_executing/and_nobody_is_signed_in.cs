// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnContext.when_executing;

/// <summary>
/// Nobody who is not signed in can give a note about the context; the giver is never taken from the request.
/// </summary>
public class and_nobody_is_signed_in : a_note_command<GiveNoteOnContext>
{
    protected override ClaimsPrincipal Principal => Givers.Anonymous();

    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnContext(_noteId));

    [Fact] void should_not_be_authorized() => _result.ShouldNotBeAuthorized();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
