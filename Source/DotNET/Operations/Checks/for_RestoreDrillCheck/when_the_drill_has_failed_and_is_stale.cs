// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_RestoreDrillCheck;

public class when_the_drill_has_failed_and_is_stale : a_check
{
    protected override IWatchdogCheck Check => new RestoreDrillCheck(_reader, "drill", "restore-drill", TimeSpan.FromHours(30));

    void Establish()
    {
        _reader.CronJobs["restore-drill"] = new CronJobInfo("restore-drill", _time.Now.AddDays(-9), _time.Now.AddHours(-50));
        _reader.Jobs.Add(new JobInfo("restore-drill-1", _time.Now.AddHours(-70), ["restore-drill"], Complete: true));
        _reader.Jobs.Add(new JobInfo("restore-drill-2", _time.Now.AddHours(-2), ["restore-drill"], Failed: true, FailureReason: "BackoffLimitExceeded"));
        _reader.Jobs.Add(new JobInfo("restore-drill-3", _time.Now.AddMinutes(-3), ["restore-drill"]));
        _reader.Jobs.Add(new JobInfo("other-1", _time.Now.AddMinutes(-1), ["other"], Failed: true, FailureReason: "Other"));
        _reader.Pods.Add(new PodInfo("drill", "restore-drill-2-abc", "Failed", _time.Now.AddHours(-2), [new ContainerInfo("report", TerminationMessage: "RESULT=FAIL snapshot=s1 password=hunter2")], Labels: new Dictionary<string, string> { ["job-name"] = "restore-drill-2" }));
    }

    async Task Because() => await Run();

    [Fact] void should_report_that_the_drill_has_not_succeeded_in_thirty_hours() => Keys.ShouldContain("restore-drill:stale");
    [Fact] void should_say_how_many_hours() => _findings.First(finding => finding.Key == "restore-drill:stale").Text.ShouldContain("has not succeeded for 50 hours");
    [Fact] void should_report_that_the_newest_finished_job_failed() => Keys.ShouldContain("restore-drill:failed");
    [Fact] void should_not_be_misled_by_a_job_that_is_still_running_or_belongs_to_another_cronjob() => _findings.First(finding => finding.Key == "restore-drill:failed").Text.ShouldContain("restore-drill-2");
    [Fact] void should_include_the_termination_message_of_the_job() => _findings.First(finding => finding.Key == "restore-drill:failed").Text.ShouldContain("RESULT=FAIL snapshot=s1");
    [Fact] void should_redact_credentials_from_that_message() => _findings.First(finding => finding.Key == "restore-drill:failed").Text.ShouldNotContain("hunter2");
}
#endif
