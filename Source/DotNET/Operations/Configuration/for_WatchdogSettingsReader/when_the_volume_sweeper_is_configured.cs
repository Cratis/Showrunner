// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_the_volume_sweeper_is_configured : Specification
{
    WatchdogSettings _settings;
    WatchdogSettings _defaults;
    WatchdogConfigurationException _error;

    void Because()
    {
        _settings = WatchdogSettingsReader.FromEnvironment(
            new Dictionary<string, string>
            {
                ["UPCLOUD_USERNAME"] = "svc",
                ["UPCLOUD_PASSWORD"] = "pw-value",
                ["MIN_AGE_SECONDS"] = "60",
                ["MAX_PER_RUN"] = "2",
                ["DRY_RUN"] = "TRUE"
            },
            "operations");
        _defaults = WatchdogSettingsReader.FromEnvironment(new Dictionary<string, string> { ["UPCLOUD_USERNAME"] = "svc", ["UPCLOUD_PASSWORD"] = "pw-value" }, "operations");
        static void Read() => WatchdogSettingsReader.FromEnvironment(new Dictionary<string, string> { ["UPCLOUD_USERNAME"] = "svc" }, "operations");
        _error = (WatchdogConfigurationException)Catch.Exception(Read);
    }

    [Fact] void should_enable_the_sweeper() => _settings.EnabledChecks.ShouldContain("volume-sweeper");
    [Fact] void should_read_the_limits() => (_settings.VolumeSweeper!.Options.MinimumAge, _settings.VolumeSweeper.Options.MaximumPerRun).ShouldEqual((TimeSpan.FromSeconds(60), 2));
    [Fact] void should_read_dry_run() => _settings.VolumeSweeper!.Options.DryRun.ShouldBeTrue();
    [Fact] void should_default_to_two_hours_ten_volumes_and_deleting() => (_defaults.VolumeSweeper!.Options.MinimumAge, _defaults.VolumeSweeper.Options.MaximumPerRun, _defaults.VolumeSweeper.Options.DryRun).ShouldEqual((TimeSpan.FromHours(2), 10, false));
    [Fact] void should_refuse_a_user_without_a_password() => _error.Problems.ShouldContain("the volume sweeper needs both UPCLOUD_USERNAME and UPCLOUD_PASSWORD");
}
#endif
