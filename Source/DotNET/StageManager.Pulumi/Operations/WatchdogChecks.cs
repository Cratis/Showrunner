// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// Decides which checks a <see cref="Watchdog"/> runs. The names are the ones the watchdog executable understands, and the cluster
/// role is granted from the same list, so a check that is not run is a read that is not allowed.
/// </summary>
internal static class WatchdogChecks
{
    /// <summary>The pods check.</summary>
    internal const string Pods = "pods";

    /// <summary>The nodes check.</summary>
    internal const string Nodes = "nodes";

    /// <summary>The volumes check.</summary>
    internal const string Volumes = "volumes";

    /// <summary>The certificates check.</summary>
    internal const string Certificates = "certificates";

    /// <summary>The CronJob freshness check.</summary>
    internal const string CronJobs = "cronjobs";

    /// <summary>The Percona Backup for MongoDB check.</summary>
    internal const string PbmBackups = "pbm-backups";

    /// <summary>The restore drill check.</summary>
    internal const string RestoreDrill = "restore-drill";

    /// <summary>The readiness check.</summary>
    internal const string Readiness = "readiness";

    /// <summary>The public URLs check.</summary>
    internal const string PublicUrls = "public-urls";

    /// <summary>The operational metrics check.</summary>
    internal const string Metrics = "metrics";

    /// <summary>The telemetry budget check.</summary>
    internal const string TelemetryBudget = "telemetry-budget";

    /// <summary>The Chronicle observers check.</summary>
    internal const string ChronicleObservers = "chronicle-observers";

    /// <summary>The GitHub Actions schedule audit.</summary>
    internal const string ActionsAudit = "actions-audit";

    /// <summary>
    /// Works out the checks to run.
    /// </summary>
    /// <param name="args">The inputs.</param>
    /// <returns>The names of the checks, without repeats.</returns>
    internal static IReadOnlyList<string> For(WatchdogArgs args)
    {
        if (args.Checks is not null)
        {
            return [.. args.Checks.Distinct(StringComparer.Ordinal)];
        }

        var checks = new List<string> { Pods, Nodes, Volumes };
        void AddWhen(bool condition, string name)
        {
            if (condition)
            {
                checks.Add(name);
            }
        }

        AddWhen(args.WatchCertificates, Certificates);
        AddWhen(args.CronJobs.Count > 0, CronJobs);
        AddWhen(args.Pbm is not null, PbmBackups);
        AddWhen(args.RestoreDrill is not null, RestoreDrill);
        AddWhen(args.Readiness is not null, Readiness);
        AddWhen(args.PublicUrls.Count > 0, PublicUrls);
        AddWhen(args.Prometheus is not null, Metrics);
        AddWhen(args.Prometheus is not null, TelemetryBudget);
        AddWhen(args.Chronicle is not null, ChronicleObservers);
        AddWhen(args.GitHubAudit is not null, ActionsAudit);
        return checks;
    }
}
