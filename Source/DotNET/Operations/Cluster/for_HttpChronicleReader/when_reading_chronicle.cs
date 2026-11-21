// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Cluster.for_HttpChronicleReader;

public class when_reading_chronicle : Specification
{
    StubHttpHandler _handler;
    IReadOnlyList<string> _stores;
    IReadOnlyList<ChronicleFailedPartition> _partitions;
    IReadOnlyList<ChronicleObserver> _observers;
    Exception _failure;

    static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    HttpResponseMessage Answer(HttpRequestMessage request) => request.RequestUri!.AbsolutePath switch
    {
        "/connect/token" => Json("{\"access_token\":\"the-token\"}"),
        "/api/event-stores/all-event-stores" => Json("{\"isSuccess\":true,\"data\":[{\"name\":\"System\"},{\"name\":\"studio\"}]}"),
        "/api/observation/get-failed-partitions" => Json("{\"isSuccess\":true,\"data\":[{\"observerId\":\"sender\",\"isResolved\":false,\"isQuarantined\":true,\"attempts\":[{\"occurred\":\"2026-07-01T10:00:00Z\"},{\"occurred\":\"2026-07-01T09:00:00Z\"}]}]}"),
        "/api/observation/all-observers" => Json("{\"isSuccess\":true,\"data\":[{\"id\":\"inbox\",\"runningState\":5}]}"),
        _ => Json("{\"isSuccess\":false,\"validationResults\":[{\"message\":\"Event store does not exist.\"}],\"exceptionMessages\":[\"boom\"]}")
    };

    async Task Because()
    {
        _handler = new StubHttpHandler(Answer);
        var reader = new HttpChronicleReader(new HttpClient(_handler), new Uri("https://chronicle:8080"), "watchdog", "shh-it-is-secret");
        var timeout = TimeSpan.FromSeconds(10);
        _stores = await reader.EventStores(timeout, CancellationToken.None);
        _partitions = await reader.FailedPartitions("studio", "acme", timeout, CancellationToken.None);
        _observers = await reader.Observers("studio", "acme", timeout, CancellationToken.None);
        try
        {
            await reader.Namespaces("studio", timeout, CancellationToken.None);
        }
        catch (Exception error)
        {
            _failure = error;
        }
    }

    [Fact] void should_not_list_the_system_store() => _stores.ShouldContainOnly("studio");
    [Fact] void should_read_the_failed_partition() => _partitions.Single().ObserverId.ShouldEqual("sender");
    [Fact] void should_read_when_it_first_failed() => _partitions.Single().FirstFailure.ShouldEqual(new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero));
    [Fact] void should_read_that_it_is_quarantined() => _partitions.Single().IsQuarantined.ShouldBeTrue();
    [Fact] void should_read_the_observer_state() => _observers.Single().IsQuarantined.ShouldBeTrue();
    [Fact] void should_fetch_one_token_for_all_requests() => _handler.Requests.Count(request => request.Uri.AbsolutePath == "/connect/token").ShouldEqual(1);
    [Fact] void should_send_the_client_credentials_only_to_the_token_endpoint() => _handler.Requests.Where(request => request.Body.Contains("shh-it-is-secret")).Select(request => request.Uri.AbsolutePath).ShouldContainOnly("/connect/token");
    [Fact] void should_authorize_queries_with_the_token() => _handler.Requests.First(request => request.Uri.AbsolutePath.StartsWith("/api/")).Authorization.ShouldEqual("Bearer the-token");
    [Fact] void should_pass_the_event_store_and_namespace() => _handler.Requests.First(request => request.Uri.AbsolutePath.EndsWith("get-failed-partitions")).Uri.Query.ShouldEqual("?eventStore=studio&namespace=acme");
    [Fact] void should_refuse_an_envelope_that_did_not_succeed() => _failure.Message.ShouldEqual("Event store does not exist.; boom");
    [Fact] void should_never_put_the_secret_in_an_error() => _failure.Message.ShouldNotContain("shh-it-is-secret");
}
#endif
