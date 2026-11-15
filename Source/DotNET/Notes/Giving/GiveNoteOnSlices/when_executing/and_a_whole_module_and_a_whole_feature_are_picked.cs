// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices.when_executing;

/// <summary>
/// A note can be about a whole module or a whole feature, not only a slice: such a level is a location whose finer parts are
/// not set, and it travels in the note exactly as picked.
/// </summary>
public class and_a_whole_module_and_a_whole_feature_are_picked : a_note_command<GiveNoteOnSlices>
{
    static readonly SliceLocation _module = new("Billing", FeatureName.NotSet, SliceName.NotSet);
    static readonly SliceLocation _feature = new("Orders", "Editing", SliceName.NotSet);

    async Task Because() => _result = await _scenario.Execute(
        new GiveNoteOnSlices(_noteId, NoteEvents.Context, [_module, _feature], NoteKind.Suggestion, "The whole area feels slow"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_keep_the_module_and_the_feature_as_picked() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnSlices, GiverGaveNoteOnSlices>(_noteId, e => e.Slices.SequenceEqual(new[] { _module, _feature }));
}
#endif
