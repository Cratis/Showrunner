// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Withdrawing.WithdrawNote.when_executing;

public class and_nobody_is_signed_in : a_note_command<WithdrawNote>
{
    protected override ClaimsPrincipal Principal => Givers.Anonymous();

    void Establish() => Given(NoteEvents.Given());

    async Task Because() => _result = await _scenario.Execute(new WithdrawNote(_noteId));

    [Fact] void should_not_be_authorized() => _result.ShouldNotBeAuthorized();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
