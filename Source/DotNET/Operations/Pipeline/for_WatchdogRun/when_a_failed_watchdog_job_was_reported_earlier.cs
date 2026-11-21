// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_failed_watchdog_job_was_reported_earlier : a_watchdog_run
{
    void Establish()
    {
        _options = _options with { WatchdogCronJob = "cluster-health-watchdog" };
        _reader.Jobs.Add(new JobInfo("retained", _time.Now.AddHours(-1), ["cluster-health-watchdog"], Failed: true, FailureReason: "DeadlineExceeded"));
        _state.Current = AlertStateSnapshot.Empty with { ReportedFailedJobs = ["retained", "pruned-away"] };
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_report_it_again() => _channel.Delivered.ShouldBeEmpty();
    [Fact] void should_keep_remembering_a_job_that_is_still_retained() => _state.Current.ReportedFailedJobs.ShouldContain("retained");
    [Fact] void should_forget_a_job_the_cluster_no_longer_keeps() => _state.Current.ReportedFailedJobs.ShouldNotContain("pruned-away");
}
#endif
