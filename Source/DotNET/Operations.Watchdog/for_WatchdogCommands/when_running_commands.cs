// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Configuration;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.State;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Watchdog.for_WatchdogCommands;

public class when_running_commands : Specification
{
    int _healthy;
    int _failing;
    int _test;
    int _testWithoutChannel;

    async Task Because()
    {
        var healthy = Factory(StubHttpHandler.Answering(HttpStatusCode.OK), "https://hc-ping.example.com/abc");
        _healthy = await WatchdogCommands.Run(WatchdogCommand.Cluster, healthy, CancellationToken.None);
        _test = await WatchdogCommands.Run(WatchdogCommand.TestAlert, healthy, CancellationToken.None);

        var broken = new FakeClusterReader();
        broken.Pods.Add(new Cluster.PodInfo("shop", "web", "Running", null, [new Cluster.ContainerInfo("web", false, 1, "CrashLoopBackOff")]));
        _failing = await WatchdogCommands.Run(WatchdogCommand.Cluster, Factory(StubHttpHandler.Answering(HttpStatusCode.InternalServerError), null, broken), CancellationToken.None);
        _testWithoutChannel = await WatchdogCommands.Run(WatchdogCommand.TestAlert, Factory(StubHttpHandler.Answering(HttpStatusCode.OK), null, includeChannel: false), CancellationToken.None);
    }

    static WatchdogFactory Factory(StubHttpHandler handler, string? heartbeat, FakeClusterReader? reader = null, bool includeChannel = true)
    {
        var environment = new Dictionary<string, string> { ["WATCHDOG_CHECKS"] = "pods" };
        if (includeChannel)
        {
            environment["DISCORD_WEBHOOK_URL"] = "https://discord.example.com/api/webhooks/1/x";
        }

        if (heartbeat is not null)
        {
            environment["WATCHDOG_HEARTBEAT_URL"] = heartbeat;
        }

        return new WatchdogFactory(
            WatchdogSettingsReader.FromEnvironment(environment, "operations"),
            reader ?? new FakeClusterReader(),
            Substitute.For<IConfigMapStore>(),
            new HttpClient(handler),
            null,
            new ManualTimeProvider { AutoFireWithin = TimeSpan.FromSeconds(30) },
            NullLoggerFactory.Instance);
    }

    [Fact] void should_exit_with_zero_when_the_pass_succeeds() => _healthy.ShouldEqual(0);
    [Fact] void should_exit_with_zero_when_the_delivery_test_is_delivered() => _test.ShouldEqual(0);
    [Fact] void should_exit_with_one_when_an_alert_cannot_be_delivered() => _failing.ShouldEqual(1);
    [Fact] void should_exit_with_one_when_the_delivery_test_has_no_channel() => _testWithoutChannel.ShouldEqual(1);
}
#endif
