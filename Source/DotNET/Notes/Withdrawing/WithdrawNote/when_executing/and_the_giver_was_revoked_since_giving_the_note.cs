// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Withdrawing.WithdrawNote.when_executing;

/// <summary>
/// Only granted users may give feedback and the desk revoked the giver's right after they gave the note: the note is still
/// theirs, so they can withdraw it.
/// </summary>
public class and_the_giver_was_revoked_since_giving_the_note : a_note_command<WithdrawNote>
{
    protected override FeedbackAccessMode AccessMode => FeedbackAccessMode.GrantedUsers;

    void Establish()
    {
        _givers.Revoke(Givers.Identity);
        Given(NoteEvents.Given());
    }

    async Task Because() => _result = await _scenario.Execute(new WithdrawNote(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_giver_withdrew_it() =>
        await _scenario.ShouldHaveAppendedEvent<WithdrawNote, GiverWithdrewNote>(_noteId);
}
#endif
