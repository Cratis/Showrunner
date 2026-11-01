// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices.when_executing;

public class and_slices_are_picked : a_note_command<GiveNoteOnSlices>
{
    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnSlices(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_picked_slices() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnSlices, GiverGaveNoteOnSlices>(_noteId, e => e.Slices.SequenceEqual([NoteEvents.Location, NoteEvents.AnotherLocation]));
}
#endif
