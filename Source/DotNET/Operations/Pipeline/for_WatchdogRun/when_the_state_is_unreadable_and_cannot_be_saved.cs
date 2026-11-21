// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_state_is_unreadable_and_cannot_be_saved : a_watchdog_run
{
    void Establish()
    {
        _state.FailLoading = true;
        _state.FailSaving = true;
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_fail_the_run() => _outcome.Succeeded.ShouldBeFalse();
    [Fact] void should_exit_with_one() => _outcome.ExitCode.ShouldEqual(1);
    [Fact] void should_not_alert_twice_for_the_same_outage() => _channel.Delivered.Count.ShouldEqual(1);
    [Fact] void should_report_the_failure_to_the_heartbeat() => _heartbeat.Pings.ShouldContainOnly(true);
}
#endif
