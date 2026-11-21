// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_run_crashes_and_the_state_can_be_saved : a_watchdog_run
{
    void Establish()
    {
        _options = _options with { WatchdogJobName = "watchdog-29000" };

        // Collection succeeds and saves; the heartbeat after a clean run is what throws, so the run crashes after saving.
        _checks.Add(StubCheck.Finding("check"));
        _heartbeat.Fail = true;
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_fail_the_run() => _outcome.Succeeded.ShouldBeFalse();
    [Fact] void should_remember_the_job_as_reported_so_it_is_not_reported_twice() => _state.Current.ReportedFailedJobs.ShouldContainOnly("watchdog-29000");
    [Fact] void should_start_the_run_failure_cooldown() => _state.Current.LastAlerted.ContainsKey("watchdog:run-failed").ShouldBeTrue();
    [Fact] void should_keep_the_state_the_run_saved_before_it_crashed() => _state.Current.LastCheck.ShouldEqual(_started);
    [Fact] void should_ping_once_as_healthy_and_once_as_failed() => _heartbeat.Pings.ShouldContainOnly(false, true);
}
#endif
