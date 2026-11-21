// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Channels;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;
using Cratis.Showrunner.Operations.State;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogFactory;

public class when_creating_a_run_from_settings : Specification
{
    FakeClusterReader _reader;
    StubHttpHandler _handler;
    IConfigMapStore _store;
    WatchdogFactory _factory;
    WatchdogOutcome _outcome;
    WatchdogOutcome _test;

    void Establish()
    {
        _reader = new FakeClusterReader();
        _reader.Pods.Add(new PodInfo("shop", "web", "Running", new DateTimeOffset(2026, 7, 1, 11, 0, 0, TimeSpan.Zero), [new ContainerInfo("web", false, 3, "CrashLoopBackOff")]));
        _handler = StubHttpHandler.Answering(HttpStatusCode.OK);
        _store = Substitute.For<IConfigMapStore>();
        var settings = WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string>
            {
                ["WATCHDOG_CHECKS"] = "pods",
                ["DIRECT_ALERTS_URL"] = "https://direct.example.com",
                ["DIRECT_ALERTS_TOKEN"] = "token-1",
                ["DISCORD_WEBHOOK_URL"] = "https://discord.example.com/api/webhooks/1/x",
                ["WATCHDOG_HEARTBEAT_URL"] = "https://hc-ping.example.com/abc"
            },
            "operations");
        _factory = new WatchdogFactory(settings, _reader, _store, new HttpClient(_handler), null, new ManualTimeProvider { AutoFireWithin = TimeSpan.FromSeconds(30) }, NullLoggerFactory.Instance);
    }

    async Task Because()
    {
        _outcome = await _factory.CreateClusterRun().Run(CancellationToken.None);
        var afterRun = _handler.Requests.Count;
        _test = await _factory.CreateTestAlertRun().RunTestAlert(CancellationToken.None);
        TestRequests = _handler.Requests.Skip(afterRun).Select(request => request.Uri.Host).ToList();
    }

    List<string> TestRequests { get; set; }

    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_deliver_to_every_configured_channel() => _handler.Requests.Select(request => request.Uri.Host).ShouldContain("direct.example.com", "discord.example.com");
    [Fact] void should_ping_the_heartbeat() => _handler.Requests.Select(request => request.Uri.Host).ShouldContain("hc-ping.example.com");
    [Fact] void should_keep_the_state_in_the_configured_config_map() => _store.Received().Write("cluster-health-watchdog-state", "state", Arg.Any<string>(), Arg.Any<CancellationToken>());
    [Fact] void should_find_the_crash_looping_pod_with_the_enabled_check() => _handler.Requests.First(request => request.Uri.Host == "direct.example.com").Body.ShouldContain("pod:shop/web:CrashLoopBackOff");
    [Fact] void should_run_the_delivery_test_through_the_same_channels() => TestRequests.ShouldContainOnly("direct.example.com", "discord.example.com");
    [Fact] void should_not_ping_the_heartbeat_in_the_delivery_test() => TestRequests.ShouldNotContain("hc-ping.example.com");
    [Fact] void should_pass_the_delivery_test() => _test.Succeeded.ShouldBeTrue();
}
#endif
