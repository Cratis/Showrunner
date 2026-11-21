// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_finding_resolves : a_watchdog_run
{
    void Establish()
    {
        _checks.Add(StubCheck.Finding("check"));
        _state.Current = StateWith("node:n1:NotReady", TimeSpan.FromHours(1));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_not_alert() => _channel.Delivered.ShouldBeEmpty();
    [Fact] void should_forget_the_cooldown() => _state.Current.LastAlerted.ShouldBeEmpty();
    [Fact] void should_forget_when_it_was_first_seen_so_a_recurrence_starts_fresh() => _state.Current.FirstSeen.ShouldBeEmpty();
    [Fact] void should_ping_the_heartbeat_as_healthy() => _heartbeat.Pings.ShouldContainOnly(false);
}
#endif
