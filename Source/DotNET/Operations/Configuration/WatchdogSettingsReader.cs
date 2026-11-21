// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Showrunner.Operations.Channels;
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// Reads <see cref="WatchdogSettings"/> from environment variables. The names the Python watchdog used are kept
/// (<c>DISCORD_WEBHOOK_URL</c>, <c>SMTP_SERVER</c>, <c>ALERT_TO</c>, <c>WATCHDOG_HEARTBEAT_URL</c>, <c>PUBLIC_URLS</c>, …) so a
/// deployment moves over without renaming; <c>DIRECT_ALERTS_URL</c>, <c>DIRECT_ALERTS_TOKEN</c> and
/// <c>DIRECT_ALERTS_SOURCE</c> are new. The reference configuration of every variable is in the Operations documentation.
/// </summary>
public static class WatchdogSettingsReader
{
    static readonly string[] _defaultChecks = [PodsCheck.CheckName, NodesCheck.CheckName, VolumesCheck.CheckName];

    static readonly string[] _knownChecks =
    [
        PodsCheck.CheckName,
        NodesCheck.CheckName,
        VolumesCheck.CheckName,
        CertificatesCheck.CheckName,
        CronJobFreshnessCheck.CheckName,
        PbmBackupsCheck.CheckName,
        RestoreDrillCheck.CheckName,
        ReadinessCheck.CheckName,
        PublicUrlsCheck.CheckName,
        OperationalMetricsCheck.CheckName,
        TelemetryBudgetCheck.CheckName,
        ChronicleObserversCheck.CheckName
    ];

    /// <summary>
    /// Reads the settings.
    /// </summary>
    /// <param name="environment">The environment variables.</param>
    /// <param name="defaultNamespace">The namespace the watchdog runs in, when <c>WATCHDOG_NAMESPACE</c> is not set.</param>
    /// <returns>The settings.</returns>
    /// <exception cref="WatchdogConfigurationException">The environment does not describe a usable watchdog.</exception>
    public static WatchdogSettings FromEnvironment(IReadOnlyDictionary<string, string> environment, string defaultNamespace)
    {
        var problems = new List<string>();
        string? Get(string name) => environment.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
        Uri? GetUrl(string name) => Url(Get(name), name, problems);
        TimeSpan Duration(string name, TimeSpan fallback) => Get(name) is { } text ? DurationOrProblem(text, name, fallback, problems) : fallback;

        var @namespace = Get("WATCHDOG_NAMESPACE") ?? defaultNamespace;
        var options = new WatchdogOptions
        {
            Namespace = @namespace,
            Deployment = Get("WATCHDOG_DEPLOYMENT"),
            RealertInterval = Duration("REALERT_SECONDS", TimeSpan.FromHours(6)),
            EscalateAfter = Duration("ESCALATE_AFTER_SECONDS", TimeSpan.FromHours(24)),
            ScheduleInterval = ScheduleInterval(Get("WATCHDOG_SCHEDULE")),
            WatchdogCronJob = Get("WATCHDOG_CRONJOB"),
            WatchdogJobName = Get("WATCHDOG_JOB_NAME"),
            Deadline = Duration("DEADLINE_SECONDS", TimeSpan.FromSeconds(240))
        };

        var cronJobs = CronJobs(Get("WATCHDOG_CRONJOBS"), @namespace, problems);
        var readiness = Readiness(Get);
        var urls = (Get("PUBLIC_URLS") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(url => Url(url, "PUBLIC_URLS", problems))
            .OfType<Uri>()
            .ToList();
        var pbm = Get("PBM_NAMESPACE") is { } pbmNamespace ? new PbmSettings(pbmNamespace, Duration("PBM_MAX_AGE", TimeSpan.FromHours(3))) : null;
        var drill = Get("RESTORE_DRILL_NAMESPACE") is { } drillNamespace
            ? new RestoreDrillSettings(drillNamespace, Get("RESTORE_DRILL_CRONJOB") ?? "restore-drill", Duration("RESTORE_DRILL_STALE", TimeSpan.FromHours(30)))
            : null;

        var settings = new WatchdogSettings
        {
            Options = options,
            StateConfigMap = Get("WATCHDOG_STATE_CONFIGMAP") ?? "cluster-health-watchdog-state",
            DiscordWebhook = GetUrl("DISCORD_WEBHOOK_URL"),
            Smtp = Smtp(Get, problems),
            Direct = Direct(Get, GetUrl("DIRECT_ALERTS_URL"), problems),
            Heartbeat = GetUrl("WATCHDOG_HEARTBEAT_URL"),
            Volumes = new VolumesCheckOptions
            {
                ThresholdPercent = int.TryParse(Get("VOLUME_THRESHOLD_PERCENT"), CultureInfo.InvariantCulture, out var threshold) ? threshold : 85,
                Advice = Get("VOLUME_ADVICE") ?? new VolumesCheckOptions().Advice
            },
            CronJobs = cronJobs,
            Pbm = pbm,
            RestoreDrill = drill,
            Readiness = readiness,
            PublicUrls = urls,
            Prometheus = GetUrl("PROMETHEUS_URL"),
            Metrics = new OperationalMetricsOptions
            {
                PlatformNamespace = Get("PLATFORM_NAMESPACE") ?? new OperationalMetricsOptions().PlatformNamespace,
                Advice = Get("VOLUME_ADVICE") ?? new OperationalMetricsOptions().Advice
            },
            Chronicle = Chronicle(Get, GetUrl("CHRONICLE_URL"), problems),
            TelemetryBudget = new TelemetryBudgetOptions
            {
                PlatformNamespace = Get("PLATFORM_NAMESPACE") ?? new TelemetryBudgetOptions().PlatformNamespace,
                SeriesThreshold = int.TryParse(Get("PROMETHEUS_SERIES_THRESHOLD"), CultureInfo.InvariantCulture, out var series) ? series : new TelemetryBudgetOptions().SeriesThreshold
            }
        };

        var enabled = EnabledChecks(Get("WATCHDOG_CHECKS"), settings, problems);
        RequireConfigurationOfEnabledChecks(enabled, settings, problems);
        if (problems.Count > 0)
        {
            throw new WatchdogConfigurationException(problems);
        }

        return settings with { EnabledChecks = enabled };
    }

    static List<string> EnabledChecks(string? explicitList, WatchdogSettings settings, List<string> problems)
    {
        if (explicitList is not null)
        {
            var names = explicitList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            problems.AddRange(names.Where(name => !_knownChecks.Contains(name)).Select(name => $"WATCHDOG_CHECKS names an unknown check '{name}' (known: {string.Join(", ", _knownChecks)})"));
            return [.. names.Where(_knownChecks.Contains).Distinct()];
        }

        // Without a list, the checks that need nothing but the Kubernetes API run, plus every check that has been configured.
        var enabled = new List<string>(_defaultChecks);
        if (settings.CronJobs.Count > 0)
        {
            enabled.Add(CronJobFreshnessCheck.CheckName);
        }

        if (settings.Pbm is not null)
        {
            enabled.Add(PbmBackupsCheck.CheckName);
        }

        if (settings.RestoreDrill is not null)
        {
            enabled.Add(RestoreDrillCheck.CheckName);
        }

        if (settings.Readiness is not null)
        {
            enabled.Add(ReadinessCheck.CheckName);
        }

        if (settings.PublicUrls.Count > 0)
        {
            enabled.Add(PublicUrlsCheck.CheckName);
        }

        if (settings.Prometheus is not null)
        {
            enabled.Add(OperationalMetricsCheck.CheckName);
        }

        if (settings.Chronicle is not null)
        {
            enabled.Add(ChronicleObserversCheck.CheckName);
        }

        return enabled;
    }

    static void RequireConfigurationOfEnabledChecks(List<string> enabled, WatchdogSettings settings, List<string> problems)
    {
        void Require(string check, bool configured, string variable)
        {
            if (enabled.Contains(check) && !configured)
            {
                problems.Add($"the '{check}' check is enabled but {variable} is not set");
            }
        }

        Require(CronJobFreshnessCheck.CheckName, settings.CronJobs.Count > 0, "WATCHDOG_CRONJOBS");
        Require(PbmBackupsCheck.CheckName, settings.Pbm is not null, "PBM_NAMESPACE");
        Require(RestoreDrillCheck.CheckName, settings.RestoreDrill is not null, "RESTORE_DRILL_NAMESPACE");
        Require(ReadinessCheck.CheckName, settings.Readiness is not null, "READINESS_NAMESPACE");
        Require(PublicUrlsCheck.CheckName, settings.PublicUrls.Count > 0, "PUBLIC_URLS");
        Require(OperationalMetricsCheck.CheckName, settings.Prometheus is not null, "PROMETHEUS_URL");
        Require(TelemetryBudgetCheck.CheckName, settings.Prometheus is not null, "PROMETHEUS_URL");
        Require(ChronicleObserversCheck.CheckName, settings.Chronicle is not null, "CHRONICLE_URL");
    }

    static TimeSpan? ScheduleInterval(string? schedule)
    {
        // Only the interval form "*/N * * * *" says how often the watchdog should run; any other schedule skips missed-run detection.
        var minutes = schedule?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return minutes?.StartsWith("*/", StringComparison.Ordinal) == true && int.TryParse(minutes[2..], CultureInfo.InvariantCulture, out var every) && every > 0
            ? TimeSpan.FromMinutes(every)
            : null;
    }

    static List<CronJobExpectation> CronJobs(string? text, string defaultNamespace, List<string> problems)
    {
        var expectations = new List<CronJobExpectation>();
        foreach (var entry in (text ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // [namespace/]name=maxAge[?]  -  a trailing ? says a missing CronJob is fine.
            var optional = entry.EndsWith('?');
            var parts = entry.TrimEnd('?').Split('=', 2);
            if (parts.Length != 2 || !DurationText.TryParse(parts[1], out var maximumAge))
            {
                problems.Add($"WATCHDOG_CRONJOBS entry '{entry}' is not [namespace/]name=duration, for example db-backups=3h");
                continue;
            }

            var qualified = parts[0].Split('/', 2);
            expectations.Add(qualified.Length == 2
                ? new CronJobExpectation(qualified[0], qualified[1], maximumAge, optional)
                : new CronJobExpectation(defaultNamespace, qualified[0], maximumAge, optional));
        }

        return expectations;
    }

    static ReadinessCheckOptions? Readiness(Func<string, string?> get)
    {
        // CORE_NAMESPACE is the Python watchdog's variable for the one workload it probed.
        var @namespace = get("READINESS_NAMESPACE") ?? get("CORE_NAMESPACE");
        if (@namespace is null)
        {
            return null;
        }

        var defaults = new ReadinessCheckOptions { Namespace = @namespace, LabelSelector = "app=core" };
        return new ReadinessCheckOptions
        {
            Namespace = @namespace,
            LabelSelector = get("READINESS_SELECTOR") ?? defaults.LabelSelector,
            ContainerName = get("READINESS_CONTAINER") ?? (get("CORE_NAMESPACE") is not null ? "core" : null),
            Port = int.TryParse(get("READINESS_PORT"), CultureInfo.InvariantCulture, out var port) ? port : defaults.Port,
            Path = get("READINESS_PATH") ?? defaults.Path,
            HangAdvice = get("READINESS_HANG_ADVICE") ?? defaults.HangAdvice
        };
    }

    static SmtpOptions? Smtp(Func<string, string?> get, List<string> problems)
    {
        string[] variables = ["SMTP_SERVER", "SMTP_PORT", "SMTP_USERNAME", "SMTP_PASSWORD", "ALERT_FROM_NAME", "ALERT_FROM", "ALERT_TO"];
        if (get("SMTP_SERVER") is not { } server)
        {
            return null;
        }

        var missing = variables.Where(name => get(name) is null).ToList();
        if (missing.Count > 0)
        {
            problems.Add($"email is configured (SMTP_SERVER) but {string.Join(", ", missing)} is not set");
            return null;
        }

        if (!int.TryParse(get("SMTP_PORT"), CultureInfo.InvariantCulture, out var port))
        {
            problems.Add("SMTP_PORT is not a number");
            return null;
        }

        return new SmtpOptions(server, port, get("SMTP_USERNAME")!, get("SMTP_PASSWORD")!, get("ALERT_FROM_NAME")!, get("ALERT_FROM")!, get("ALERT_TO")!);
    }

    static ChronicleSettings? Chronicle(Func<string, string?> get, Uri? url, List<string> problems)
    {
        if (url is null)
        {
            return null;
        }

        var clientId = get("CHRONICLE_CLIENT_ID");
        var clientSecret = get("CHRONICLE_CLIENT_SECRET");
        if (clientId is null || clientSecret is null)
        {
            problems.Add("Chronicle needs CHRONICLE_CLIENT_ID and CHRONICLE_CLIENT_SECRET together with CHRONICLE_URL");
            return null;
        }

        return new ChronicleSettings(url, clientId, clientSecret, get("CHRONICLE_CA_CERTIFICATE"));
    }

    static DirectAlertsOptions? Direct(Func<string, string?> get, Uri? url, List<string> problems)
    {
        var token = get("DIRECT_ALERTS_TOKEN");
        if (url is null && token is null)
        {
            return null;
        }

        if (url is null || token is null)
        {
            problems.Add("Cratis Direct needs both DIRECT_ALERTS_URL and DIRECT_ALERTS_TOKEN");
            return null;
        }

        return new DirectAlertsOptions(url, token, get("DIRECT_ALERTS_SOURCE"));
    }

    static Uri? Url(string? text, string variable, List<string> problems)
    {
        if (text is null)
        {
            return null;
        }

        if (Uri.TryCreate(text, UriKind.Absolute, out var url) && (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps))
        {
            return url;
        }

        // The value is not echoed: a webhook URL is a capability.
        problems.Add($"{variable} is not an absolute http(s) URL");
        return null;
    }

    static TimeSpan DurationOrProblem(string text, string variable, TimeSpan fallback, List<string> problems)
    {
        if (DurationText.TryParse(text, out var duration))
        {
            return duration;
        }

        problems.Add($"{variable} is not a duration such as 90s, 30m, 3h or a number of seconds");
        return fallback;
    }
}
