// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices.when_executing;

/// <summary>
/// The signed-in user gives a note about slices: the giver and the application come from the identity and the options.
/// </summary>
public class and_the_giver_is_signed_in : a_note_command<GiveNoteOnSlices>
{
    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnSlices(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_take_the_giver_from_the_signed_in_identity() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnSlices, GiverGaveNoteOnSlices>(_noteId, e => e.Giver == Givers.Giver);

    [Fact]
    async Task should_name_the_application_from_the_options() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnSlices, GiverGaveNoteOnSlices>(_noteId, e => e.Application == NoteEvents.Application);

    [Fact]
    async Task should_append_the_text_the_giver_wrote() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnSlices, GiverGaveNoteOnSlices>(_noteId, e => e.Kind == NoteEvents.OnSlices(_noteId).Kind && e.Text == NoteEvents.OnSlices(_noteId).Text);
}
#endif
