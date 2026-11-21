// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_HealthchecksHeartbeat;

public class when_the_first_attempt_fails : Specification
{
    StubHttpHandler _handler;

    async Task Because()
    {
        var attempt = 0;
        _handler = new StubHttpHandler(_ => ++attempt == 1 ? throw new HttpRequestException("https://hc-ping.example.com/abc-123 unreachable") : new HttpResponseMessage(HttpStatusCode.OK));
        await new HealthchecksHeartbeat(new HttpClient(_handler), new Uri("https://hc-ping.example.com/abc-123")).Ping(false, CancellationToken.None);
    }

    [Fact] void should_try_a_second_time() => _handler.Requests.Count.ShouldEqual(2);
}
#endif
