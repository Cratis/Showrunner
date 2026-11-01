// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

public class when_slices_are_picked : a_read_model<MyNote>
{
    MyNote _note;

    async Task Because()
    {
        await Happened(new GiverGaveNoteOnSlices(NoteEvents.Application, Givers.Giver, NoteEvents.Context, [NoteEvents.Location, NoteEvents.AnotherLocation], NoteKind.Suggestion, "Merge them."));
        _note = _scenario.Instance!;
    }

    [Fact] void should_be_about_slices() => _note.Target.ShouldEqual(NoteTarget.Slices);
    [Fact] void should_list_the_slices() => _note.Slices.ShouldContainOnly(NoteEvents.Location, NoteEvents.AnotherLocation);
}
#endif
