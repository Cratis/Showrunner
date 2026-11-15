// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Microsoft.AspNetCore.Http;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollectingCalls;

/// <summary>
/// The publication is known but its tenant has no namespace in the collecting store: it is told so, not given an empty list
/// that would read as nobody being granted.
/// </summary>
public class when_asking_for_the_access_of_a_tenant_without_a_namespace : note_collecting_calls
{
    int _status;

    async Task Because()
    {
        _publications.Resolve(new PublicationId("pub-lost"), Secret).Returns(Task.FromResult<NotesPublication?>(new NotesPublication("no-such-tenant", "Ledger")));
        _status = StatusOf(await _calls.Access("pub-lost", Request("Bearer " + Secret)));
    }

    [Fact] void should_answer_unprocessable() => _status.ShouldEqual(422);
}
#endif
