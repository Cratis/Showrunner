// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text.Json;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.Transport.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport.for_HttpCollectorClient;

public class when_calling_the_collector : Specification
{
    const string Secret = "s3cret-value-of-the-publication";

    StubHandler _handler;
    HttpCollectorClient _client;
    ulong? _position;
    IReadOnlyList<NoteResponse> _responses;
    Exception _refused;

    static HttpResponseMessage Answer(HttpRequestMessage request)
    {
        if (request.Method != HttpMethod.Get)
        {
            return new(HttpStatusCode.Unauthorized);
        }

        return request.RequestUri!.AbsolutePath.EndsWith("/responses", StringComparison.Ordinal)
            ? StubHandler.Json("[{\"cursor\":4,\"noteId\":\"" + Guid.Empty + "\",\"kind\":\"Addressed\",\"occurred\":\"2026-10-26T14:15:00+00:00\",\"version\":\"2.0.0\"}]")
            : StubHandler.Json("{\"sequenceNumber\":41}");
    }

    void Establish()
    {
        _handler = new(Answer);
        var options = Options.Create(new NotesOptions
        {
            Transport = NotesTransport.Http,
            PublicationId = "pub 7f3a",
            CollectorUrl = "https://studio.internal:8443/",
            CollectorToken = Secret
        });
        _client = new(new HttpClient(_handler), new CollectorToken(options), options);
    }

    async Task Because()
    {
        _position = await _client.GetPosition("fjord-logistics");
        _responses = await _client.GetResponses(3);
        _refused = await Record.ExceptionAsync(() => _client.Send("fjord-logistics", [], CancellationToken.None));
    }

    [Fact] void should_read_the_position() => _position.ShouldEqual(41ul);
    [Fact] void should_ask_for_the_position_of_the_publication_and_the_source() =>
        _handler.Requests[0].Uri.AbsoluteUri.ShouldEqual("https://studio.internal:8443/.cratis/notes/v1/publications/pub%207f3a/notes/position?source=fjord-logistics");
    [Fact] void should_ask_for_the_answers_after_the_cursor() =>
        _handler.Requests[1].Uri.AbsoluteUri.ShouldEqual("https://studio.internal:8443/.cratis/notes/v1/publications/pub%207f3a/responses?after=3");
    [Fact] void should_read_the_answers() => _responses.Single().Version.ShouldEqual("2.0.0");
    [Fact] void should_authenticate_every_call_with_the_bearer_secret() => _handler.Requests.All(_ => _.Authorization == "Bearer " + Secret).ShouldBeTrue();
    [Fact] void should_post_to_the_notes_route() => _handler.Requests[2].Uri.AbsolutePath.ShouldEqual("/.cratis/notes/v1/publications/pub%207f3a/notes");
    [Fact] void should_refuse_with_only_the_status_when_the_collector_refuses() => _refused.ShouldBeOfExactType<CollectorRequestFailed>();
    [Fact] void should_not_put_the_secret_in_the_failure() => _refused.ToString().ShouldNotContain(Secret);
}
#endif
