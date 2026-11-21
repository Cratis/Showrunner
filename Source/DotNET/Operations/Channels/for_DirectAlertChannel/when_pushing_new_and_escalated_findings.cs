// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_DirectAlertChannel;

public class when_pushing_new_and_escalated_findings : Specification
{
    StubHttpHandler _handler;
    List<JsonNode> _payloads;

    async Task Because()
    {
        _handler = StubHttpHandler.Answering(HttpStatusCode.OK);
        var channel = new DirectAlertChannel(new HttpClient(_handler), new DirectAlertsOptions(new Uri("https://direct.example.com/base/"), "token-1", "kelda"));
        await channel.Deliver(
            new AlertNotification(
                "kelda-prod",
                [new Finding("pod:shop/web:Pending", "shop/web has been Pending (open since 2026-07-01 12:00 UTC, 0h)", AlertSeverity.Warning)],
                [new Finding("node:n1:NotReady", "node n1 is not Ready", AlertSeverity.Warning)],
                [new Finding("ongoing", "still there")],
                ["resolved"]),
            CancellationToken.None);
        _payloads = [.. _handler.Requests.Select(request => JsonNode.Parse(request.Body)!)];
    }

    [Fact] void should_push_one_delivery_per_finding_that_needs_a_human() => _handler.Requests.Count.ShouldEqual(2);
    [Fact] void should_not_push_what_is_merely_ongoing() => _payloads.Select(payload => (string)payload["fingerprint"]!).ShouldNotContain("ongoing");
    [Fact] void should_not_push_what_resolved() => _payloads.Select(payload => (string)payload["fingerprint"]!).ShouldNotContain("resolved");
    [Fact] void should_post_to_the_alert_webhook_under_the_base_url() => _handler.Requests.TrueForAll(request => request.Uri.ToString() == "https://direct.example.com/base/webhooks/alerts").ShouldBeTrue();
    [Fact] void should_authenticate_with_the_bearer_token_of_the_source() => _handler.Requests.TrueForAll(request => request.Authorization == "Bearer token-1").ShouldBeTrue();
    [Fact] void should_use_the_key_of_the_finding_as_the_stable_fingerprint() => ((string)_payloads[0]["fingerprint"]!).ShouldEqual("pod:shop/web:Pending");
    [Fact] void should_title_the_alert_with_the_deployment_and_the_key() => ((string)_payloads[0]["title"]!).ShouldEqual("[kelda-prod] pod:shop/web:Pending");
    [Fact] void should_send_the_text_as_the_summary() => ((string)_payloads[0]["summary"]!).ShouldContain("shop/web has been Pending");
    [Fact] void should_send_the_severity_directly_understands() => ((string)_payloads[0]["severity"]!).ShouldEqual("warning");
    [Fact] void should_label_the_alert_with_the_deployment() => ((string)_payloads[0]["labels"]!["deployment"]!).ShouldEqual("kelda-prod");
    [Fact] void should_name_the_source() => ((string)_payloads[0]["source"]!).ShouldEqual("kelda");
    [Fact] void should_push_an_escalated_finding_as_critical() => ((string)_payloads[1]["severity"]!).ShouldEqual("critical");
    [Fact] void should_keep_the_fingerprint_of_an_escalated_finding_so_it_stays_the_same_alert() => ((string)_payloads[1]["fingerprint"]!).ShouldEqual("node:n1:NotReady");
}
#endif
