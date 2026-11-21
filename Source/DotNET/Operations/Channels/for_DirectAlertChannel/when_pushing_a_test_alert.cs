// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_DirectAlertChannel;

public class when_pushing_a_test_alert : Specification
{
    JsonNode _payload;

    async Task Because()
    {
        var handler = StubHttpHandler.Answering(HttpStatusCode.OK);
        var channel = new DirectAlertChannel(new HttpClient(handler), new DirectAlertsOptions(new Uri("https://direct.example.com"), "token-1"));
        await channel.Deliver(new AlertNotification("kelda", [new Finding("test", "TEST ALERT", AlertSeverity.Information)], [], [], [], IsTest: true), CancellationToken.None);
        _payload = JsonNode.Parse(handler.Requests.Single().Body)!;
    }

    [Fact] void should_use_a_fingerprint_that_never_collides_with_a_finding() => ((string)_payload["fingerprint"]!).ShouldEqual("watchdog:test-alert");
    [Fact] void should_be_information() => ((string)_payload["severity"]!).ShouldEqual("information");
}
#endif
