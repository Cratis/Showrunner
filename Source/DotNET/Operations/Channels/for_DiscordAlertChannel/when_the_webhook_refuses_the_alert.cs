// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_DiscordAlertChannel;

public class when_the_webhook_refuses_the_alert : Specification
{
    Exception _error;

    async Task Because()
    {
        var channel = new DiscordAlertChannel(new HttpClient(StubHttpHandler.Answering(HttpStatusCode.Forbidden)), new Uri("https://discord.example.com/api/webhooks/1/secret"));
        _error = await Catch.Exception(() => channel.Deliver(new AlertNotification("shop", [new Finding("a", "a")], [], [], []), CancellationToken.None));
    }

    [Fact] void should_fail_so_the_delivery_can_tell_the_other_channels_apart() => _error.ShouldBeOfExactType<HttpRequestException>();
}
#endif
