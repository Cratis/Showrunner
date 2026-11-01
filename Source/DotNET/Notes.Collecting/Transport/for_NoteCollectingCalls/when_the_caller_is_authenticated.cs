// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollectingCalls;

public class when_the_caller_is_authenticated : note_collecting_calls
{
    const string Bearer = "Bearer " + Secret;

    int _received;
    ulong? _positionAfter;
    int _mismatch;
    int _garbage;
    int _tooMany;
    int _responses;
    ulong? _positionOfAnotherSource;

    async Task Because()
    {
        var received = await _calls.Receive(PublicationName, Request(Bearer, NoteFor(Tenant)));
        _received = StatusOf(received);
        _positionAfter = ((JsonHttpResult<NotesPosition>)received).Value!.SequenceNumber;
        _mismatch = StatusOf(await _calls.Receive(PublicationName, Request(Bearer, NoteFor("another-tenant"))));
        _garbage = StatusOf(await _calls.Receive(PublicationName, Request(Bearer, "{ not json")));
        _tooMany = StatusOf(await _calls.Receive(PublicationName, Request(Bearer, "[" + string.Join(',', Enumerable.Repeat("{}", NoteCollectingCalls.MostNotesPerRequest + 1)) + "]")));
        _responses = StatusOf(await _calls.Responses(PublicationName, Request(Bearer, query: "?after=0")));
        _positionOfAnotherSource = ((JsonHttpResult<NotesPosition>)await _calls.Position(PublicationName, Request(Bearer, query: "?source=Default"))).Value!.SequenceNumber;
    }

    [Fact] void should_accept_the_note() => _received.ShouldEqual(200);
    [Fact] void should_answer_the_position_it_reached() => _positionAfter.ShouldEqual(0ul);
    [Fact] void should_reject_a_note_for_another_tenant_as_a_conflict() => _mismatch.ShouldEqual(409);
    [Fact] void should_reject_a_body_that_is_not_notes() => _garbage.ShouldEqual(400);
    [Fact] void should_reject_too_many_notes() => _tooMany.ShouldEqual(413);
    [Fact] void should_answer_the_responses() => _responses.ShouldEqual(200);
    [Fact] void should_read_the_source_from_the_query() => _positionOfAnotherSource.ShouldBeNull();
}
#endif
