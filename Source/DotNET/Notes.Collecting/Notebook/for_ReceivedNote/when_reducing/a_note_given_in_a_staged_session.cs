// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.given;
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;

namespace Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.when_reducing;

/// <summary>
/// A note is published until its marker says it was given in a staged session: then it is staged, and answering it afterwards
/// keeps it so. The marker of a note that was never received changes nothing.
/// </summary>
public class a_note_given_in_a_staged_session : a_note_being_reduced
{
    ReceivedNote _opened;
    ReceivedNote _marked;
    ReceivedNote _answered;
    ReceivedNote _neverReceived;

    void Because()
    {
        _opened = ReceivedNote.Reduce(ReceivedNotes.OnContext, null, Context);
        _marked = ReceivedNote.Reduce(new NoteOnStagedApplicationReceived(), _opened)!;
        _answered = ReceivedNote.Reduce(new OperatorAcknowledgedNote(), _marked)!;
        _neverReceived = ReceivedNote.Reduce(new NoteOnStagedApplicationReceived(), null);
    }

    [Fact] void should_be_published_until_marked() => _opened.IsStaged.ShouldBeFalse();
    [Fact] void should_be_staged_once_marked() => _marked.IsStaged.ShouldBeTrue();
    [Fact] void should_stay_staged_when_answered() => _answered.IsStaged.ShouldBeTrue();
    [Fact] void should_change_nothing_else_when_marked() => (_marked with { IsStaged = false }).ShouldEqual(_opened);
    [Fact] void should_not_open_a_note_that_was_never_received() => _neverReceived.ShouldBeNull();
}
#endif
