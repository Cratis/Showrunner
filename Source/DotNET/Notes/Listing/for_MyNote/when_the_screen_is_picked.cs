// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

public class when_the_screen_is_picked : a_read_model<MyNote>
{
    MyNote _note;

    async Task Because()
    {
        await Happened(new GiverGaveNoteOnScreen(NoteEvents.Application, Givers.Giver, NoteEvents.Context, NoteKind.Question, "Why?"));
        _note = _scenario.Instance!;
    }

    [Fact] void should_be_about_the_screen() => _note.Target.ShouldEqual(NoteTarget.Screen);
}
#endif
