// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.given;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;

namespace Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.when_reducing;

public class a_withdrawn_note_that_is_answered : a_note_being_reduced
{
    ReceivedNote _withdrawn;
    ReceivedNote _acknowledged;
    ReceivedNote _addressed;
    ReceivedNote _declined;

    void Establish() => _withdrawn = Opened with { IsWithdrawn = true };

    void Because()
    {
        _acknowledged = ReceivedNote.Reduce(new OperatorAcknowledgedNote(), _withdrawn)!;
        _addressed = ReceivedNote.Reduce(new OperatorAddressedNote("2.0.0"), _withdrawn)!;
        _declined = ReceivedNote.Reduce(new OperatorDeclinedNote("Out of scope."), _withdrawn)!;
    }

    [Fact] void should_stay_withdrawn_when_acknowledged() => _acknowledged.IsWithdrawn.ShouldBeTrue();
    [Fact] void should_stay_withdrawn_when_addressed() => _addressed.IsWithdrawn.ShouldBeTrue();
    [Fact] void should_stay_withdrawn_when_declined() => _declined.IsWithdrawn.ShouldBeTrue();
}
#endif
