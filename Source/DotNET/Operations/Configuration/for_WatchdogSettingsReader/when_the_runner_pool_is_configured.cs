// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_the_runner_pool_is_configured : Specification
{
    WatchdogSettings _settings;
    WatchdogSettings _defaults;
    WatchdogConfigurationException _error;

    void Because()
    {
        _settings = WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string>
            {
                ["RUNNER_NAMESPACE"] = "arc-runners",
                ["RUNNER_SCALE_SETS"] = "big, small,release",
                ["MAX_JOB_MINUTES"] = "0",
                ["PENDING_MINUTES"] = "20"
            },
            "operations");
        _defaults = WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string> { ["RUNNER_NAMESPACE"] = "arc-runners", ["RUNNER_SCALE_SETS"] = "big" },
            "operations");
        static void Read() => WatchdogSettingsReader.FromEnvironment(new Dictionary<string, string> { ["RUNNER_NAMESPACE"] = "arc-runners" }, "operations");
        _error = (WatchdogConfigurationException)Catch.Exception(Read);
    }

    [Fact] void should_enable_the_runners_check() => _settings.EnabledChecks.ShouldContain("runners");
    [Fact] void should_read_the_scale_sets() => _settings.Runners!.ScaleSets.ShouldContainOnly("big", "small", "release");
    [Fact] void should_read_a_limit_of_zero_as_report_only() => _settings.Runners!.MaximumJobTime.ShouldEqual(TimeSpan.Zero);
    [Fact] void should_read_the_pending_time() => _settings.Runners!.PendingTime.ShouldEqual(TimeSpan.FromMinutes(20));
    [Fact] void should_default_the_job_limit_to_ninety_minutes() => _defaults.Runners!.MaximumJobTime.ShouldEqual(TimeSpan.FromMinutes(90));
    [Fact] void should_default_the_pending_time_to_thirty_minutes() => _defaults.Runners!.PendingTime.ShouldEqual(TimeSpan.FromMinutes(30));
    [Fact] void should_refuse_a_namespace_without_scale_sets() => _error.Problems.ShouldContain("RUNNER_NAMESPACE needs RUNNER_SCALE_SETS, the comma separated scale sets to watch");
}
#endif
