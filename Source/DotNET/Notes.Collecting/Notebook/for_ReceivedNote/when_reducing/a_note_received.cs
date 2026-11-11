// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.given;

namespace Cratis.Showrunner.Notes.Collecting.Notebook.for_ReceivedNote.when_reducing;

public class a_note_received : a_note_being_reduced
{
    ReceivedNote[] _opened;
    ReceivedNote[] _kept;
    ReceivedNote _current;

    void Establish() => _current = new(TheNote, true, true, "1.0.0", false);

    void Because()
    {
        _opened =
        [
            ReceivedNote.Reduce(ReceivedNotes.OnContext, null, Context),
            ReceivedNote.Reduce(OnSlices, null, Context),
            ReceivedNote.Reduce(OnScreen, null, Context),
            ReceivedNote.Reduce(OnComponent, null, Context),
            ReceivedNote.Reduce(PropertyChange, null, Context)
        ];
        _kept =
        [
            ReceivedNote.Reduce(ReceivedNotes.OnContext, _current, Context),
            ReceivedNote.Reduce(OnSlices, _current, Context),
            ReceivedNote.Reduce(OnScreen, _current, Context),
            ReceivedNote.Reduce(OnComponent, _current, Context),
            ReceivedNote.Reduce(PropertyChange, _current, Context)
        ];
    }

    [Fact] void should_open_a_note_for_each_kind() => _opened.Length.ShouldEqual(5);
    [Fact] void should_open_it_as_received_and_nothing_more() => _opened.ShouldEachConformTo(_ => _ == Opened);
    [Fact] void should_keep_the_state_the_note_has() => _kept.ShouldEachConformTo(_ => ReferenceEquals(_, _current));
}
#endif
