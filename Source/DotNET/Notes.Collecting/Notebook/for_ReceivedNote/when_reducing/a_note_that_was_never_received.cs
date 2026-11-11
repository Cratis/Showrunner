// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.given;
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;

namespace Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.when_reducing;

public class a_note_that_was_never_received : a_note_being_reduced
{
    ReceivedNote? _withdrawn;
    ReceivedNote? _acknowledged;
    ReceivedNote? _addressed;
    ReceivedNote? _declined;

    void Because()
    {
        _withdrawn = ReceivedNote.Reduce(new NoteWithdrawalReceived(), null);
        _acknowledged = ReceivedNote.Reduce(new OperatorAcknowledgedNote(), null);
        _addressed = ReceivedNote.Reduce(new OperatorAddressedNote("2.0.0"), null);
        _declined = ReceivedNote.Reduce(new OperatorDeclinedNote("Out of scope."), null);
    }

    [Fact] void should_not_create_it_from_a_withdrawal() => _withdrawn.ShouldBeNull();
    [Fact] void should_not_create_it_from_an_acknowledgement() => _acknowledged.ShouldBeNull();
    [Fact] void should_not_create_it_from_an_address() => _addressed.ShouldBeNull();
    [Fact] void should_not_create_it_from_a_decline() => _declined.ShouldBeNull();
}
#endif
