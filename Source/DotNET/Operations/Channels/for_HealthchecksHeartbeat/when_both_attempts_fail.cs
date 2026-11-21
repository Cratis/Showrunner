// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_HealthchecksHeartbeat;

public class when_both_attempts_fail : Specification
{
    StubHttpHandler _handler;
    Exception _error;

    async Task Because()
    {
        _handler = StubHttpHandler.Answering(HttpStatusCode.InternalServerError);
        var heartbeat = new HealthchecksHeartbeat(new HttpClient(_handler), new Uri("https://hc-ping.example.com/abc-123"));
        _error = await Catch.Exception(() => heartbeat.Ping(false, CancellationToken.None));
    }

    [Fact] void should_stop_after_two_attempts() => _handler.Requests.Count.ShouldEqual(2);
    [Fact] void should_fail() => _error.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_never_put_the_capability_url_in_the_error() => _error.Message.ShouldNotContain("abc-123");
}
#endif
