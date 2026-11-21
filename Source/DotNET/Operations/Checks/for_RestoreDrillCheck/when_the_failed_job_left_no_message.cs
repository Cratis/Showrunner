// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_RestoreDrillCheck;

public class when_the_failed_job_left_no_message : a_check
{
    protected override IWatchdogCheck Check => new RestoreDrillCheck(_reader, "drill", "restore-drill", TimeSpan.FromHours(30));

    void Establish()
    {
        _reader.CronJobs["restore-drill"] = new CronJobInfo("restore-drill", _time.Now.AddDays(-9), _time.Now.AddHours(-20));
        _reader.Jobs.Add(new JobInfo("restore-drill-1", _time.Now.AddHours(-1), ["restore-drill"], Failed: true, FailureReason: "BackoffLimitExceeded"));
    }

    async Task Because() => await Run();

    [Fact] void should_still_report_the_failed_job_with_its_own_reason() => _findings.Single().Text.ShouldContain("failed (BackoffLimitExceeded): no termination message");
}
#endif
