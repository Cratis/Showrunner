// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.given;
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;

namespace Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.when_reducing;

public class a_note_answered : a_note_being_reduced
{
    ReceivedNote _withdrawn;
    ReceivedNote _acknowledged;
    ReceivedNote _addressed;
    ReceivedNote _declined;
    ReceivedNote _addressedAfterDecline;
    ReceivedNote _declinedAfterAddress;

    void Because()
    {
        _withdrawn = ReceivedNote.Reduce(new NoteWithdrawalReceived(), Opened)!;
        _acknowledged = ReceivedNote.Reduce(new OperatorAcknowledgedNote(), Opened)!;
        _addressed = ReceivedNote.Reduce(new OperatorAddressedNote("2.0.0"), Opened)!;
        _declined = ReceivedNote.Reduce(new OperatorDeclinedNote("Out of scope."), Opened)!;
        _addressedAfterDecline = ReceivedNote.Reduce(new OperatorAddressedNote("2.0.0"), _declined)!;
        _declinedAfterAddress = ReceivedNote.Reduce(new OperatorDeclinedNote("Out of scope."), _addressed)!;
    }

    [Fact] void should_only_withdraw_when_withdrawn() => _withdrawn.ShouldEqual(Opened with { IsWithdrawn = true });
    [Fact] void should_only_acknowledge_when_acknowledged() => _acknowledged.ShouldEqual(Opened with { IsAcknowledged = true });
    [Fact] void should_note_the_version_when_addressed() => _addressed.ShouldEqual(Opened with { AddressedInVersion = "2.0.0" });
    [Fact] void should_not_decline_when_addressed() => _addressed.IsDeclined.ShouldBeFalse();
    [Fact] void should_clear_the_version_when_declined() => _declined.AddressedInVersion.ShouldEqual(AddressedInVersion.NotSet);
    [Fact] void should_decline_when_declined() => _declined.IsDeclined.ShouldBeTrue();
    [Fact] void should_leave_the_other_state_when_declined() => _declined.ShouldEqual(Opened with { IsDeclined = true });
    [Fact] void should_undecline_when_addressed_after_a_decline() => _addressedAfterDecline.ShouldEqual(Opened with { AddressedInVersion = "2.0.0" });
    [Fact] void should_clear_the_version_when_declined_after_an_address() => _declinedAfterAddress.ShouldEqual(Opened with { IsDeclined = true });
}
#endif
