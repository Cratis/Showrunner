// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Probe.for_HealthchecksProbeSink;

public class when_reporting_to_healthchecks : Specification
{
    static readonly ProbeReport _ok = new([[]], [], []);
    static readonly ProbeReport _down = new([[]], [new ProbeResult("direct", "health", false, 503, null, 1)], []);

    StubHttpHandler _healthy;
    StubHttpHandler _failing;
    StubHttpHandler _flaky;
    StubHttpHandler _broken;
    Exception _failure;

    async Task Because()
    {
        var url = new Uri("https://hc-ping.example/secret-uuid/");
        _healthy = StubHttpHandler.Answering(HttpStatusCode.OK);
        await new HealthchecksProbeSink(new HttpClient(_healthy), url).Report(_ok, CancellationToken.None);

        _failing = StubHttpHandler.Answering(HttpStatusCode.OK);
        await new HealthchecksProbeSink(new HttpClient(_failing), url).Report(_down, CancellationToken.None);

        var calls = 0;
        _flaky = new StubHttpHandler(_ => new HttpResponseMessage(++calls < 3 ? HttpStatusCode.BadGateway : HttpStatusCode.OK));
        await new HealthchecksProbeSink(new HttpClient(_flaky), url).Report(_ok, CancellationToken.None);

        _broken = StubHttpHandler.Answering(HttpStatusCode.InternalServerError);
        _failure = await Record.ExceptionAsync(() => new HealthchecksProbeSink(new HttpClient(_broken), url).Report(_down, CancellationToken.None));
    }

    [Fact] void should_ping_the_check_with_a_get_when_everything_answers() => (_healthy.Requests.Single().Method, _healthy.Requests.Single().Uri.AbsoluteUri).ShouldEqual((HttpMethod.Get, "https://hc-ping.example/secret-uuid/"));
    [Fact] void should_post_to_the_fail_endpoint_when_something_is_down() => (_failing.Requests.Single().Method, _failing.Requests.Single().Uri.AbsoluteUri).ShouldEqual((HttpMethod.Post, "https://hc-ping.example/secret-uuid/fail"));
    [Fact] void should_send_the_failing_endpoints_as_the_body() => _failing.Requests.Single().Body.ShouldEqual("direct/health: 503");
    [Fact] void should_retry_an_idempotent_ping() => _flaky.Requests.Count.ShouldEqual(3);
    [Fact] void should_give_up_after_four_attempts() => _broken.Requests.Count.ShouldEqual(4);
    [Fact] void should_fail_without_the_ping_address() => _failure.Message.ShouldEqual("healthchecks delivery failed");
}
#endif
