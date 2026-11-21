// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_HttpDirectClient;

public class when_direct_answers : Specification
{
    static readonly DirectCredentials _credentials = new("instance-credential", 4);

    StubHttpHandler _handler;
    HttpStatusCode _status;
    DirectOutcome _ok;
    DirectOutcome _noContent;
    DirectOutcome _fenced;
    DirectOutcome _unauthorized;
    DirectOutcome _rejected;
    DirectOutcome _unavailable;
    DirectOutcome _unreachable;
    HttpClient _http;
    HttpDirectClient _client;

    void Establish()
    {
        _status = HttpStatusCode.OK;
        _handler = new StubHttpHandler(_ => new HttpResponseMessage(_status) { Content = new StringContent(DirectFixtures.Receipt) });
        _http = new HttpClient(_handler);
        _client = new HttpDirectClient(_http, new Uri("https://direct.example.com/"), "prod edge");
    }

    async Task Because()
    {
        _ok = (await _client.Events(_credentials, new WatchdogEventBatch([]), CancellationToken.None)).Outcome;
        _status = HttpStatusCode.NoContent;
        _noContent = (await _client.Configuration(_credentials, 3, true, CancellationToken.None)).Outcome;
        _status = HttpStatusCode.Conflict;
        _fenced = (await _client.Events(_credentials, new WatchdogEventBatch([]), CancellationToken.None)).Outcome;
        _status = HttpStatusCode.Unauthorized;
        _unauthorized = (await _client.Events(_credentials, new WatchdogEventBatch([]), CancellationToken.None)).Outcome;
        _status = HttpStatusCode.BadRequest;
        _rejected = (await _client.Events(_credentials, new WatchdogEventBatch([]), CancellationToken.None)).Outcome;
        _status = HttpStatusCode.ServiceUnavailable;
        _unavailable = (await _client.Events(_credentials, new WatchdogEventBatch([]), CancellationToken.None)).Outcome;
        _unreachable = (await new HttpDirectClient(new HttpClient(new StubHttpHandler(_ => throw new HttpRequestException("refused"))), new Uri("https://direct.example.com"), "prod").Events(_credentials, new WatchdogEventBatch([]), CancellationToken.None)).Outcome;
    }

    [Fact] void should_call_the_route_of_the_instance_with_its_name_escaped() => _handler.Requests[0].Uri.AbsoluteUri.ShouldEqual("https://direct.example.com/webhooks/watchdogs/prod%20edge/events");
    [Fact] void should_ask_for_the_configuration_after_a_revision_with_a_renewal() => _handler.Requests[1].Uri.PathAndQuery.ShouldEqual("/webhooks/watchdogs/prod%20edge/configuration?after=3&renew=true");
    [Fact] void should_send_the_credential_as_a_bearer_token() => _handler.Requests[0].Authorization.ShouldEqual("Bearer instance-credential");
    [Fact] void should_map_200_to_ok() => _ok.ShouldEqual(DirectOutcome.Ok);
    [Fact] void should_map_204_to_nothing_newer() => _noContent.ShouldEqual(DirectOutcome.NoContent);
    [Fact] void should_map_409_to_fenced() => _fenced.ShouldEqual(DirectOutcome.Fenced);
    [Fact] void should_map_401_to_unauthorized() => _unauthorized.ShouldEqual(DirectOutcome.Unauthorized);
    [Fact] void should_map_400_to_rejected() => _rejected.ShouldEqual(DirectOutcome.Rejected);
    [Fact] void should_map_503_to_unavailable() => _unavailable.ShouldEqual(DirectOutcome.Unavailable);
    [Fact] void should_map_a_connection_failure_to_unavailable_instead_of_throwing() => _unreachable.ShouldEqual(DirectOutcome.Unavailable);
}
#endif
