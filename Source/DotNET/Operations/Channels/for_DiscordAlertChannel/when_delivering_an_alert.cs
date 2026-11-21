// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Channels.for_DiscordAlertChannel;

public class when_delivering_an_alert : Specification
{
    StubHttpHandler _handler;
    JsonNode _payload;

    async Task Because()
    {
        _handler = StubHttpHandler.Answering(HttpStatusCode.NoContent);
        var channel = new DiscordAlertChannel(new HttpClient(_handler), new Uri("https://discord.example.com/api/webhooks/1/secret"));
        await channel.Deliver(new AlertNotification("shop", [new Finding("a", new string('x', 6000), AlertSeverity.Critical)], [], [], []), CancellationToken.None);
        _payload = JsonNode.Parse(_handler.Requests.Single().Body)!;
    }

    [Fact] void should_post_once() => _handler.Requests.Single().Method.ShouldEqual(HttpMethod.Post);
    [Fact] void should_post_to_the_webhook() => _handler.Requests.Single().Uri.AbsolutePath.ShouldEqual("/api/webhooks/1/secret");
    [Fact] void should_use_a_user_agent_cloudflare_accepts() => _handler.Requests.Single().UserAgent.ShouldEqual("cratis-cluster-health-watchdog/1.0");
    [Fact] void should_title_the_embed_with_the_subject() => ((string)_payload["embeds"]![0]!["title"]!).ShouldEqual("[shop] 1 cluster issue(s) need attention");
    [Fact] void should_keep_the_description_inside_discords_limit() => ((string)_payload["embeds"]![0]!["description"]!).Length.ShouldEqual(4000);
    [Fact] void should_color_a_critical_alert_red() => ((int)_payload["embeds"]![0]!["color"]!).ShouldEqual(0xE74C3C);
}
#endif
