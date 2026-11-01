// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

public class when_a_note_is_given : a_read_model<MyNote>
{
    MyNote _note;

    async Task Because()
    {
        await Happened(NoteEvents.Given());
        _note = _scenario.Instance!;
    }

    [Fact] void should_be_the_note() => _note.Id.ShouldEqual(_noteId);
    [Fact] void should_belong_to_the_giver() => _note.GivenBy.ShouldEqual(Givers.UserId);
    [Fact] void should_be_about_the_context() => _note.Target.ShouldEqual(NoteTarget.Context);
    [Fact] void should_have_the_kind() => _note.Kind.ShouldEqual(NoteKind.Problem);
    [Fact] void should_have_the_text() => _note.Text.ShouldEqual(new MyNoteText("The invoice is not sent."));
    [Fact] void should_have_the_location() => _note.Location.ShouldEqual(NoteEvents.Location);
    [Fact] void should_have_the_screen() => _note.Screen.ShouldEqual(NoteEvents.Context.Screen);
    [Fact] void should_be_given() => _note.Status.ShouldEqual(NoteStatus.Given);
    [Fact] void should_have_no_status_detail() => _note.StatusDetail.ShouldEqual(NoteStatusDetail.NotSet);
    [Fact] void should_be_given_when_it_happened() => _note.GivenAt.ShouldEqual(At(0));
}
#endif
