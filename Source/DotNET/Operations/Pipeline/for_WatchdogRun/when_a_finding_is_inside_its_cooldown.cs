// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_finding_is_inside_its_cooldown : a_watchdog_run
{
    void Establish()
    {
        FindsEveryPass(new Finding("node:n1:NotReady", "node n1 is not Ready"));
        _state.Current = StateWith("node:n1:NotReady", TimeSpan.FromHours(1));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_not_alert() => _channel.Delivered.ShouldBeEmpty();
    [Fact] void should_keep_the_original_alert_time() => _state.Current.LastAlerted["node:n1:NotReady"].ShouldEqual(_started - TimeSpan.FromHours(1));
    [Fact] void should_keep_the_open_since_time() => _state.Current.FirstSeen["node:n1:NotReady"].ShouldEqual(_started - TimeSpan.FromHours(1));
    [Fact] void should_still_ping_the_heartbeat() => _heartbeat.Pings.ShouldContainOnly(false);
}
#endif
