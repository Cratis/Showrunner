// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Withdrawing.WithdrawNote.when_executing;

public class and_the_note_is_already_withdrawn : a_note_command<WithdrawNote>
{
    void Establish() => Given(NoteEvents.Given(), new GiverWithdrewNote());

    async Task Because() => _result = await _scenario.Execute(new WithdrawNote(_noteId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
