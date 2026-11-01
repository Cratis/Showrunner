// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Withdrawing.WithdrawNote.when_executing;

public class and_the_note_is_open : a_note_command<WithdrawNote>
{
    void Establish() => Given(NoteEvents.Given());

    async Task Because() => _result = await _scenario.Execute(new WithdrawNote(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_giver_withdrew_it() =>
        await _scenario.ShouldHaveAppendedEvent<WithdrawNote, GiverWithdrewNote>(_noteId);
}
#endif
