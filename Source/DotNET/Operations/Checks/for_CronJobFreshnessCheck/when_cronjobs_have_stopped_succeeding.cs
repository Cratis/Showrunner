// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_CronJobFreshnessCheck;

public class when_cronjobs_have_stopped_succeeding : a_check
{
    static readonly TimeSpan ThreeHours = TimeSpan.FromHours(3);

    protected override IWatchdogCheck Check => new CronJobFreshnessCheck(_reader, [
        new CronJobExpectation("ops", "db-backups", ThreeHours),
        new CronJobExpectation("ops", "db-backups-offsite", ThreeHours),
        new CronJobExpectation("ops", "new-deploy", ThreeHours),
        new CronJobExpectation("ops", "never-succeeded", ThreeHours),
        new CronJobExpectation("ops", "runner-watchdog", TimeSpan.FromMinutes(30), Optional: true),
        new CronJobExpectation("ops", "required-but-gone", ThreeHours)]);

    void Establish()
    {
        _reader.CronJobs["db-backups"] = new CronJobInfo("db-backups", _time.Now.AddDays(-9), _time.Now.AddHours(-2));
        _reader.CronJobs["db-backups-offsite"] = new CronJobInfo("db-backups-offsite", _time.Now.AddDays(-9), _time.Now.AddHours(-4));
        _reader.CronJobs["new-deploy"] = new CronJobInfo("new-deploy", _time.Now.AddHours(-1), null);
        _reader.CronJobs["never-succeeded"] = new CronJobInfo("never-succeeded", _time.Now.AddHours(-5), null);
    }

    async Task Because() => await Run();

    [Fact] void should_not_report_a_cronjob_that_succeeded_recently() => Keys.ShouldNotContain("db-backups:stale");
    [Fact] void should_report_a_cronjob_that_has_not_succeeded_within_its_maximum_age() => Keys.ShouldContain("db-backups-offsite:stale");
    [Fact] void should_say_how_long_and_since_when() => _findings.First(finding => finding.Key == "db-backups-offsite:stale").Text.ShouldContain("has not succeeded for 240 minutes");
    [Fact] void should_measure_a_new_cronjob_from_its_creation_so_a_fresh_deploy_is_not_flagged() => Keys.ShouldNotContain("new-deploy:stale");
    [Fact] void should_not_exempt_a_cronjob_that_never_succeeds_indefinitely() => Keys.ShouldContain("never-succeeded:stale");
    [Fact] void should_ignore_an_optional_cronjob_that_does_not_exist() => Keys.ShouldNotContain("runner-watchdog:api");
    [Fact] void should_report_a_required_cronjob_that_does_not_exist() => Keys.ShouldContain("required-but-gone:api");
}
#endif
