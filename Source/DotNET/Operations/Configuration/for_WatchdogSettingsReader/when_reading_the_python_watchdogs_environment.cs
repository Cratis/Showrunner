// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_WatchdogSettingsReader;

public class when_reading_the_python_watchdogs_environment : Specification
{
    WatchdogSettings _settings;

    void Because() => _settings = WatchdogSettingsReader.FromEnvironment(
        new Dictionary<string, string>
        {
            ["WATCHDOG_NAMESPACE"] = "operations",
            ["REALERT_SECONDS"] = "21600",
            ["DISCORD_WEBHOOK_URL"] = "https://discord.example.com/api/webhooks/1/secret",
            ["SMTP_SERVER"] = "smtp.example.com",
            ["SMTP_PORT"] = "587",
            ["SMTP_USERNAME"] = "user",
            ["SMTP_PASSWORD"] = "pass",
            ["ALERT_FROM_NAME"] = "Watchdog",
            ["ALERT_FROM"] = "watchdog@example.com",
            ["ALERT_TO"] = "ops@example.com",
            ["WATCHDOG_HEARTBEAT_URL"] = "https://hc-ping.example.com/abc",
            ["WATCHDOG_SCHEDULE"] = "*/5 * * * *",
            ["WATCHDOG_CRONJOB"] = "cluster-health-watchdog",
            ["CORE_NAMESPACE"] = "core",
            ["PUBLIC_URLS"] = "https://a.example.com/, https://b.example.com/"
        },
        "fallback");

    [Fact] void should_use_the_namespace_of_the_watchdog() => _settings.Options.Namespace.ShouldEqual("operations");
    [Fact] void should_read_the_realert_interval_in_seconds() => _settings.Options.RealertInterval.ShouldEqual(TimeSpan.FromHours(6));
    [Fact] void should_default_the_escalation_to_a_day() => _settings.Options.EscalateAfter.ShouldEqual(TimeSpan.FromHours(24));
    [Fact] void should_default_the_deadline_to_the_jobs_240_seconds() => _settings.Options.Deadline.ShouldEqual(TimeSpan.FromSeconds(240));
    [Fact] void should_read_the_schedule_interval_to_detect_missed_runs() => _settings.Options.ScheduleInterval.ShouldEqual(TimeSpan.FromMinutes(5));
    [Fact] void should_read_the_discord_webhook() => _settings.DiscordWebhook!.Host.ShouldEqual("discord.example.com");
    [Fact] void should_read_the_smtp_settings() => (_settings.Smtp!.Server, _settings.Smtp.Port, _settings.Smtp.To).ShouldEqual(("smtp.example.com", 587, "ops@example.com"));
    [Fact] void should_read_the_heartbeat() => _settings.Heartbeat!.Host.ShouldEqual("hc-ping.example.com");
    [Fact] void should_not_configure_direct_unless_asked() => (_settings.Direct is null).ShouldBeTrue();
    [Fact] void should_read_the_core_namespace_as_the_readiness_workload() => _settings.Readiness!.Namespace.ShouldEqual("core");
    [Fact] void should_probe_the_core_container_as_the_python_watchdog_did() => _settings.Readiness!.ContainerName.ShouldEqual("core");
    [Fact] void should_split_the_public_urls() => _settings.PublicUrls.Count.ShouldEqual(2);
    [Fact] void should_enable_the_basic_checks_and_every_configured_one() => _settings.EnabledChecks.ShouldContainOnly("pods", "nodes", "volumes", "readiness", "public-urls");
}
#endif
