// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent.when_executing;

/// <summary>
/// Only granted users may give feedback, and the desk granted the giver the right: the note is given as it is when everyone may.
/// </summary>
public class and_the_giver_is_granted : a_note_command<GiveNoteOnComponent>
{
    protected override FeedbackAccessMode AccessMode => FeedbackAccessMode.GrantedUsers;

    void Establish() => _givers.Grant(Givers.Identity);

    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnComponent(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_note_as_the_signed_in_giver() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnComponent, GiverGaveNoteOnComponent>(_noteId, e => e.Giver == Givers.Giver);
}
#endif
