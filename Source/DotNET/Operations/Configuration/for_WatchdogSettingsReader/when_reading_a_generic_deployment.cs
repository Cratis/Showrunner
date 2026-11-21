// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_reading_a_generic_deployment : Specification
{
    WatchdogSettings _settings;

    void Because() => _settings = WatchdogSettingsReader.FromEnvironment(
        new Dictionary<string, string>
        {
            ["WATCHDOG_CHECKS"] = "pods,nodes",
            ["DIRECT_ALERTS_URL"] = "https://direct.example.com",
            ["DIRECT_ALERTS_TOKEN"] = "token-1",
            ["DIRECT_ALERTS_SOURCE"] = "kelda",
            ["WATCHDOG_DEPLOYMENT"] = "kelda-prod"
        },
        "kelda");

    [Fact] void should_run_only_the_checks_it_names() => _settings.EnabledChecks.ShouldContainOnly("pods", "nodes");
    [Fact] void should_fall_back_to_the_namespace_it_runs_in() => _settings.Options.Namespace.ShouldEqual("kelda");
    [Fact] void should_name_the_deployment_in_alerts() => _settings.Options.DeploymentName.ShouldEqual("kelda-prod");
    [Fact] void should_configure_direct() => (_settings.Direct!.BaseUrl.Host, _settings.Direct.Token, _settings.Direct.Source).ShouldEqual(("direct.example.com", "token-1", "kelda"));
    [Fact] void should_have_no_other_channel() => (_settings.DiscordWebhook is null && _settings.Smtp is null && _settings.Heartbeat is null).ShouldBeTrue();
    [Fact] void should_not_hard_code_any_namespace_or_hostname() => (_settings.Pbm is null && _settings.RestoreDrill is null && _settings.Readiness is null && _settings.PublicUrls.Count == 0 && _settings.CronJobs.Count == 0).ShouldBeTrue();
}
#endif
