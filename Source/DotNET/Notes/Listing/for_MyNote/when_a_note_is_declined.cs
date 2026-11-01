// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Receiving;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

public class when_a_note_is_declined : a_read_model<MyNote>
{
    MyNote _note;

    async Task Because()
    {
        await Happened(NoteEvents.Given(), new ReceiverDeclinedNote("Out of scope."));
        _note = _scenario.Instance!;
    }

    [Fact] void should_be_declined() => _note.Status.ShouldEqual(NoteStatus.Declined);
    [Fact] void should_tell_why() => _note.StatusDetail.ShouldEqual(new NoteStatusDetail("Out of scope."));
}
#endif
