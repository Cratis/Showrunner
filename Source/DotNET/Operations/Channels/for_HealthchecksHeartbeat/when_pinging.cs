// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_HealthchecksHeartbeat;

public class when_pinging : Specification
{
    StubHttpHandler _handler;

    async Task Because()
    {
        _handler = StubHttpHandler.Answering(HttpStatusCode.OK);
        var heartbeat = new HealthchecksHeartbeat(new HttpClient(_handler), new Uri("https://hc-ping.example.com/abc-123/"));
        await heartbeat.Ping(false, CancellationToken.None);
        await heartbeat.Ping(true, CancellationToken.None);
    }

    [Fact] void should_post_to_the_check_when_healthy() => _handler.Requests[0].Uri.ToString().ShouldEqual("https://hc-ping.example.com/abc-123");
    [Fact] void should_post_to_fail_when_the_run_failed() => _handler.Requests[1].Uri.ToString().ShouldEqual("https://hc-ping.example.com/abc-123/fail");
    [Fact] void should_post() => _handler.Requests.TrueForAll(request => request.Method == HttpMethod.Post).ShouldBeTrue();
}
#endif
