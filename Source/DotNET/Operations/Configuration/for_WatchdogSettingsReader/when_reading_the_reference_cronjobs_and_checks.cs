// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_reading_the_reference_cronjobs_and_checks : Specification
{
    WatchdogSettings _settings;

    void Because() => _settings = WatchdogSettingsReader.FromEnvironment(
        new Dictionary<string, string>
        {
            ["WATCHDOG_CRONJOBS"] = "db-backups=3h,databases/db-backups-offsite=26h,runner-watchdog=30m?",
            ["PBM_NAMESPACE"] = "databases",
            ["RESTORE_DRILL_NAMESPACE"] = "databases",
            ["WATCHDOG_CHECKS"] = "pods,certificates,cronjobs,pbm-backups,restore-drill"
        },
        "operations");

    [Fact] void should_read_a_cronjob_in_the_default_namespace() => _settings.CronJobs.First(job => job.Name == "db-backups").ShouldEqual(new Checks.CronJobExpectation("operations", "db-backups", TimeSpan.FromHours(3)));
    [Fact] void should_read_a_cronjob_in_another_namespace() => _settings.CronJobs.First(job => job.Name == "db-backups-offsite").Namespace.ShouldEqual("databases");
    [Fact] void should_read_an_optional_cronjob() => _settings.CronJobs.First(job => job.Name == "runner-watchdog").Optional.ShouldBeTrue();
    [Fact] void should_default_the_pbm_age_to_three_hours() => _settings.Pbm!.MaximumAge.ShouldEqual(TimeSpan.FromHours(3));
    [Fact] void should_default_the_restore_drill_to_thirty_hours() => (_settings.RestoreDrill!.CronJob, _settings.RestoreDrill.StaleAfter).ShouldEqual(("restore-drill", TimeSpan.FromHours(30)));
    [Fact] void should_enable_exactly_the_named_checks() => _settings.EnabledChecks.ShouldContainOnly("pods", "certificates", "cronjobs", "pbm-backups", "restore-drill");
}
#endif
