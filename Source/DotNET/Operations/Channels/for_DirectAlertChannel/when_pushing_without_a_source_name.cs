// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_DirectAlertChannel;

public class when_pushing_without_a_source_name : Specification
{
    JsonNode _payload;

    async Task Because()
    {
        var handler = StubHttpHandler.Answering(HttpStatusCode.OK);
        var channel = new DirectAlertChannel(new HttpClient(handler), new DirectAlertsOptions(new Uri("https://direct.example.com"), "token-1"));
        await channel.Deliver(new AlertNotification("kelda", [new Finding(new string('k', 400), "text", AlertSeverity.Critical)], [], [], []), CancellationToken.None);
        _payload = JsonNode.Parse(handler.Requests.Single().Body)!;
    }

    [Fact] void should_leave_the_source_to_the_token() => (_payload["source"] is null).ShouldBeTrue();
    [Fact] void should_keep_the_title_within_a_sensible_length() => ((string)_payload["title"]!).Length.ShouldEqual(200);
    [Fact] void should_keep_the_whole_key_as_the_fingerprint() => ((string)_payload["fingerprint"]!).Length.ShouldEqual(400);
    [Fact] void should_send_critical_findings_as_critical() => ((string)_payload["severity"]!).ShouldEqual("critical");
}
#endif
