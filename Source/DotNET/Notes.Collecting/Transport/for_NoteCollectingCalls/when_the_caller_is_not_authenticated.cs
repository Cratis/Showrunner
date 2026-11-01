// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Microsoft.Net.Http.Headers;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollectingCalls;

/// <summary>
/// A missing, malformed, wrong or unknown credential all answer the same, and nothing is stored or revealed.
/// </summary>
public class when_the_caller_is_not_authenticated : note_collecting_calls
{
    int[] _statuses;
    int _statusForAnUnknownPublication;
    string _challenge;

    async Task Because()
    {
        var challenged = Request(null);
        var results = new[]
        {
            await _calls.Receive(PublicationName, Request(null, NoteFor(Tenant))),
            await _calls.Receive(PublicationName, Request("Basic abc", NoteFor(Tenant))),
            await _calls.Receive(PublicationName, Request("Bearer ", NoteFor(Tenant))),
            await _calls.Receive(PublicationName, Request("Bearer wrong-secret", NoteFor(Tenant))),
            await _calls.Position(PublicationName, Request("Bearer wrong-secret")),
            await _calls.Responses(PublicationName, Request("Bearer wrong-secret"))
        };
        _statuses = [.. results.Select(StatusOf)];
        _statusForAnUnknownPublication = StatusOf(await _calls.Position("pub-unknown", Request("Bearer " + Secret)));
        await _calls.Position(PublicationName, challenged);
        _challenge = challenged.Response.Headers[HeaderNames.WWWAuthenticate].ToString();
    }

    [Fact] void should_answer_unauthorized_every_time() => _statuses.ShouldContainOnly(401, 401, 401, 401, 401, 401);
    [Fact] void should_answer_the_same_for_another_publications_secret() => _statusForAnUnknownPublication.ShouldEqual(401);
    [Fact] void should_ask_for_a_bearer_token() => _challenge.ShouldEqual("Bearer");
    [Fact] async Task should_store_nothing() => (await _tenantStore.EventLog.GetFromSequenceNumber(Cratis.Chronicle.Events.EventSequenceNumber.First)).Count.ShouldEqual(0);
    [Fact] async Task should_hand_the_resolver_the_publication_and_the_trimmed_token() => await _publications.Received().Resolve(new PublicationId(PublicationName), "wrong-secret");
}
#endif
