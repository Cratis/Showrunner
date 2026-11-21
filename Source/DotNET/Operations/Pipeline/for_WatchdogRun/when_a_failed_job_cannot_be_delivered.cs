// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_failed_job_cannot_be_delivered : a_watchdog_run
{
    void Establish()
    {
        FindsEveryPass(new Finding("disk", "disk full"));
        _channel.Fail = true;
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_stamp_the_cooldown_so_the_next_run_tries_again() => _state.Current.LastAlerted.ShouldBeEmpty();
    [Fact] void should_not_mark_any_failed_job_as_reported() => _state.Current.ReportedFailedJobs.ShouldBeEmpty();
    [Fact] void should_fail_the_run() => _outcome.Succeeded.ShouldBeFalse();
}
#endif
