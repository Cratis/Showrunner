// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The environment variables a <see cref="Watchdog"/> job runs with, split into what is not secret and what is. The names are the
/// ones the watchdog executable reads.
/// </summary>
internal static class WatchdogEnvironment
{
    /// <summary>The Discord webhook URL, a secret.</summary>
    internal const string DiscordKey = "DISCORD_WEBHOOK_URL";

    /// <summary>The SMTP user, a secret.</summary>
    internal const string SmtpUsernameKey = "SMTP_USERNAME";

    /// <summary>The SMTP password, a secret.</summary>
    internal const string SmtpPasswordKey = "SMTP_PASSWORD";

    /// <summary>The bearer token of the Direct alert source, a secret.</summary>
    internal const string DirectTokenKey = "DIRECT_ALERTS_TOKEN";

    /// <summary>The heartbeat URL, a secret.</summary>
    internal const string HeartbeatKey = "WATCHDOG_HEARTBEAT_URL";

    /// <summary>
    /// Builds the variables that are not secret.
    /// </summary>
    /// <param name="kubernetesName">The Kubernetes name of the watchdog, which is also the name of its CronJob.</param>
    /// <param name="args">The inputs.</param>
    /// <param name="checks">The checks that run.</param>
    /// <returns>The variables, in a stable order.</returns>
    internal static IReadOnlyList<KeyValuePair<string, string>> Plain(string kubernetesName, WatchdogArgs args, IReadOnlyList<string> checks)
    {
        var variables = new List<KeyValuePair<string, string>>
        {
            new("WATCHDOG_CRONJOB", kubernetesName),
            new("WATCHDOG_STATE_CONFIGMAP", $"{kubernetesName}-state"),
            new("WATCHDOG_SCHEDULE", args.Schedule ?? Watchdog.DefaultSchedule),
            new("DEADLINE_SECONDS", (args.DeadlineSeconds ?? Watchdog.DefaultDeadlineSeconds).ToString(CultureInfo.InvariantCulture)),
            new("WATCHDOG_CHECKS", string.Join(',', checks))
        };

        void Add(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                variables.Add(new(name, value));
            }
        }

        Add("WATCHDOG_DEPLOYMENT", args.Deployment);
        Add("REALERT_SECONDS", args.RealertInterval);
        Add("ESCALATE_AFTER_SECONDS", args.EscalateAfter);
        Add("VOLUME_THRESHOLD_PERCENT", args.VolumeThresholdPercent?.ToString(CultureInfo.InvariantCulture));
        Add("VOLUME_ADVICE", args.VolumeAdvice);
        Add("PUBLIC_URLS", args.PublicUrls.Count > 0 ? string.Join(',', args.PublicUrls) : null);
        Add("WATCHDOG_CRONJOBS", args.CronJobs.Count > 0 ? string.Join(',', args.CronJobs.Select(CronJob)) : null);

        if (args.Smtp is { } smtp)
        {
            Add("SMTP_SERVER", smtp.Server);
            Add("SMTP_PORT", (smtp.Port ?? Watchdog.DefaultSmtpPort).ToString(CultureInfo.InvariantCulture));
            Add("ALERT_FROM_NAME", smtp.FromName);
            Add("ALERT_FROM", smtp.From);
            Add("ALERT_TO", smtp.To);
        }

        Add("DIRECT_ALERTS_URL", args.Direct?.Url);
        Add("DIRECT_ALERTS_SOURCE", args.Direct?.Source);

        if (args.Pbm is { } pbm)
        {
            Add("PBM_NAMESPACE", pbm.Namespace);
            Add("PBM_MAX_AGE", pbm.MaximumAge);
        }

        if (args.RestoreDrill is { } drill)
        {
            Add("RESTORE_DRILL_NAMESPACE", drill.Namespace);
            Add("RESTORE_DRILL_CRONJOB", drill.CronJob);
            Add("RESTORE_DRILL_STALE", drill.StaleAfter);
        }

        if (args.Readiness is { } readiness)
        {
            Add("READINESS_NAMESPACE", readiness.Namespace);
            Add("READINESS_SELECTOR", readiness.Selector);
            Add("READINESS_CONTAINER", readiness.Container);
            Add("READINESS_PORT", readiness.Port?.ToString(CultureInfo.InvariantCulture));
            Add("READINESS_PATH", readiness.Path);
            Add("READINESS_HANG_ADVICE", readiness.HangAdvice);
        }

        return variables;
    }

    static string CronJob(WatchdogCronJobArgs job) =>
        $"{(job.Namespace is null ? string.Empty : job.Namespace + "/")}{job.Name}={job.MaximumAge}{(job.Optional ? "?" : string.Empty)}";
}
