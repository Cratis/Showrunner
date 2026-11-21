// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_DirectAlertChannel;

public class when_the_direct_refuses_the_alert : Specification
{
    Exception _error;

    async Task Because()
    {
        var channel = new DirectAlertChannel(new HttpClient(StubHttpHandler.Answering(HttpStatusCode.Unauthorized)), new DirectAlertsOptions(new Uri("https://direct.example.com"), "token-1"));
        _error = await Catch.Exception(() => channel.Deliver(new AlertNotification("kelda", [new Finding("a", "a")], [], [], []), CancellationToken.None));
    }

    [Fact] void should_fail_so_a_wrong_token_is_visible() => _error.ShouldBeOfExactType<HttpRequestException>();
    [Fact] void should_not_put_the_token_in_the_error() => _error.Message.ShouldNotContain("token-1");
}
#endif
