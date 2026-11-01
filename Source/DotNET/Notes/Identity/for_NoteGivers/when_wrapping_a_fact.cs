// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Identity.for_NoteGivers;

public class when_wrapping_a_fact : Specification
{
    NoteId _noteId;
    object _fact;
    Cratis.Chronicle.EventSequences.EventForEventSourceId _wrapped;

    void Because()
    {
        _noteId = NoteId.New();
        _fact = NoteEvents.Given();
        _wrapped = NoteGivers.Fact(_noteId, Givers.UserId, _fact);
    }

    [Fact] void should_append_it_on_the_note() => _wrapped.EventSourceId.Value.ShouldEqual(_noteId.Value.ToString());
    [Fact] void should_carry_the_fact() => _wrapped.Event.ShouldEqual(_fact);
    [Fact] void should_have_the_giver_as_subject() => _wrapped.Subject.ShouldEqual(NoteGivers.For(Givers.UserId));
}
#endif
