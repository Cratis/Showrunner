// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Receiving;
using Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

/// <summary>
/// Once withdrawn, a note stays withdrawn: an answer that crossed the withdrawal does not bring it back.
/// </summary>
public class when_a_note_is_withdrawn : a_read_model<MyNote>
{
    MyNote _note;

    async Task Because()
    {
        await Happened(NoteEvents.Given(), new GiverWithdrewNote(), new ReceiverAddressedNote("2.0.0"));
        _note = _scenario.Instance!;
    }

    [Fact] void should_stay_withdrawn() => _note.Status.ShouldEqual(NoteStatus.Withdrawn);
    [Fact] void should_not_take_the_version() => _note.StatusDetail.ShouldEqual(NoteStatusDetail.NotSet);
}
#endif
