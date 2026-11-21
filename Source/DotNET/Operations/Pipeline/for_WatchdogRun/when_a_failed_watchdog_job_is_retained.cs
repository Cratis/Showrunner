// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_failed_watchdog_job_is_retained : a_watchdog_run
{
    void Establish()
    {
        _options = _options with { WatchdogCronJob = "cluster-health-watchdog" };
        _reader.Jobs.Add(new JobInfo("cluster-health-watchdog-1", _time.Now.AddHours(-1), ["cluster-health-watchdog"], _time.Now.AddHours(-1), Failed: true, FailureReason: "DeadlineExceeded", FailureMessage: "Job was active longer than specified deadline"));
        _reader.Jobs.Add(new JobInfo("other-cron-1", _time.Now.AddHours(-1), ["other"], Failed: true, FailureReason: "BackoffLimitExceeded"));
        _reader.Jobs.Add(new JobInfo("cluster-health-watchdog-2", _time.Now.AddMinutes(-5), ["cluster-health-watchdog"]));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_report_the_failed_job() => Notification.Raised.Select(finding => finding.Key).ShouldContainOnly("watchdog:failed-run:cluster-health-watchdog-1");
    [Fact] void should_include_the_reason_and_message() => Notification.Raised.Single().Text.ShouldContain("DeadlineExceeded: Job was active longer than specified deadline");
    [Fact] void should_ignore_jobs_of_other_cronjobs() => Notification.Raised.Select(finding => finding.Key).ShouldNotContain("watchdog:failed-run:other-cron-1");
    [Fact] void should_remember_it_was_reported() => _state.Current.ReportedFailedJobs.ShouldContainOnly("cluster-health-watchdog-1");
}
#endif
