// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_run_crashes_inside_the_failure_cooldown : a_watchdog_run
{
    void Establish()
    {
        _checks.Add(StubCheck.Finding("check"));
        _heartbeat.Fail = true;
        _state.Current = StateWith("watchdog:run-failed", TimeSpan.FromHours(1));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_fail_the_run() => _outcome.Succeeded.ShouldBeFalse();
    [Fact] void should_not_alert_again() => _channel.Delivered.ShouldBeEmpty();
    [Fact] void should_keep_the_cooldown_across_a_successful_collection() => _state.Current.LastAlerted.ContainsKey("watchdog:run-failed").ShouldBeTrue();
}
#endif
