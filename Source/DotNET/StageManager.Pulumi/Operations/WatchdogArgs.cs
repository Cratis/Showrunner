// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The inputs of a <see cref="Watchdog"/>.
/// </summary>
public sealed class WatchdogArgs
{
    /// <summary>
    /// Gets the Kubernetes provider the resources are created with, or null for the default one.
    /// </summary>
    public KubernetesProvider? Provider { get; init; }

    /// <summary>
    /// Gets the Kubernetes name of the watchdog, or null to use the Pulumi name of the component.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the namespace the watchdog runs in. Its state ConfigMap and its own Jobs live there.
    /// </summary>
    public required Input<string> Namespace { get; init; }

    /// <summary>
    /// Gets the name of the deployment shown in alert subjects, or null to use the namespace.
    /// </summary>
    public string? Deployment { get; init; }

    /// <summary>
    /// Gets the image the CronJob runs, or null for <c>cratis/showrunner-watchdog-image:latest</c>.
    /// </summary>
    public Input<string>? Image { get; init; }

    /// <summary>
    /// Gets the cron schedule, or null for every five minutes. Only the <c>*/N * * * *</c> form lets the watchdog notice that it
    /// stopped running.
    /// </summary>
    public string? Schedule { get; init; }

    /// <summary>
    /// Gets how long a run may take before Kubernetes kills it, in seconds, or null for 240.
    /// </summary>
    public int? DeadlineSeconds { get; init; }

    /// <summary>
    /// Gets how long an alerted finding stays quiet before it is alerted again, or null for <c>6h</c>.
    /// </summary>
    public string? RealertInterval { get; init; }

    /// <summary>
    /// Gets how long a finding may stay open before it is escalated, or null for <c>24h</c>.
    /// </summary>
    public string? EscalateAfter { get; init; }

    /// <summary>
    /// Gets the checks to run, by name (<c>pods</c>, <c>nodes</c>, <c>volumes</c>, <c>certificates</c>, <c>cronjobs</c>,
    /// <c>pbm-backups</c>, <c>restore-drill</c>, <c>readiness</c>, <c>public-urls</c>), or null to run the basic three plus every
    /// check that has been configured below. The cluster role grants exactly what the chosen checks read.
    /// </summary>
    public IList<string>? Checks { get; init; }

    /// <summary>
    /// Gets the Discord channel, or null for none.
    /// </summary>
    public WatchdogDiscordArgs? Discord { get; init; }

    /// <summary>
    /// Gets the email channel, or null for none.
    /// </summary>
    public WatchdogSmtpArgs? Smtp { get; init; }

    /// <summary>
    /// Gets the Cratis Direct channel, or null for none.
    /// </summary>
    public WatchdogDirectArgs? Direct { get; init; }

    /// <summary>
    /// Gets the Healthchecks URL pinged after every run (and on <c>/fail</c> when a run fails), or null for no heartbeat. It is
    /// a capability and is written to a Secret as a Pulumi secret.
    /// </summary>
    public Input<string>? HeartbeatUrl { get; init; }

    /// <summary>
    /// Gets the CronJobs that must keep succeeding; the <c>cronjobs</c> check is enabled when there are any.
    /// </summary>
    public IList<WatchdogCronJobArgs> CronJobs { get; init; } = [];

    /// <summary>
    /// Gets the Percona Backup for MongoDB backups to watch, or null for none.
    /// </summary>
    public WatchdogPbmArgs? Pbm { get; init; }

    /// <summary>
    /// Gets the restore drill to watch, or null for none.
    /// </summary>
    public WatchdogRestoreDrillArgs? RestoreDrill { get; init; }

    /// <summary>
    /// Gets the workload whose readiness is probed, or null for none.
    /// </summary>
    public WatchdogReadinessArgs? Readiness { get; init; }

    /// <summary>
    /// Gets the public URLs to probe the way a user reaches the product.
    /// </summary>
    public IList<string> PublicUrls { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the <c>certificates</c> check runs. It needs cert-manager's custom resources.
    /// </summary>
    public bool WatchCertificates { get; init; }

    /// <summary>
    /// Gets the usage, in percent, above which a volume is a finding (capped at 85 by the watchdog), or null for 85.
    /// </summary>
    public int? VolumeThresholdPercent { get; init; }

    /// <summary>
    /// Gets what a volume finding tells the reader to do, such as a link to a runbook, or null for the default.
    /// </summary>
    public string? VolumeAdvice { get; init; }

    /// <summary>
    /// Gets the Prometheus to read, or null for none. It turns on the <c>metrics</c> and <c>telemetry-budget</c> checks.
    /// </summary>
    public WatchdogPrometheusArgs? Prometheus { get; init; }

    /// <summary>
    /// Gets the Chronicle whose observers are watched, or null for none. It turns on the <c>chronicle-observers</c> check.
    /// </summary>
    public WatchdogChronicleArgs? Chronicle { get; init; }

    /// <summary>
    /// Gets the GitHub organization whose Actions schedules are audited, or null for none. It turns on the <c>actions-audit</c> check.
    /// </summary>
    public WatchdogGitHubAuditArgs? GitHubAudit { get; init; }

    /// <summary>
    /// Gets the runner pool to keep from filling up with hung work, or null for none. It is a CronJob of its own.
    /// </summary>
    public WatchdogRunnersArgs? Runners { get; init; }

    /// <summary>
    /// Gets the agent harness whose stuck worker Jobs are cleaned up, or null for none. It is a CronJob of its own.
    /// </summary>
    public WatchdogAgentHarnessArgs? AgentHarness { get; init; }

    /// <summary>
    /// Gets the cloud account swept for orphaned volumes, or null for none. It is a CronJob of its own.
    /// </summary>
    public WatchdogVolumeSweeperArgs? VolumeSweeper { get; init; }

    /// <summary>
    /// Gets the synthetic probe of the public endpoints, or null for none. It is a CronJob of its own.
    /// </summary>
    public WatchdogProbeArgs? Probe { get; init; }

    /// <summary>
    /// Gets a value indicating whether a suspended CronJob is declared next to the watchdog, so a delivery test is
    /// <c>kubectl create job --from=cronjob/NAME-test-alert test-alert</c>. It defaults to <see langword="true"/>.
    /// </summary>
    public bool TestAlertJob { get; init; } = true;

    /// <summary>
    /// Gets the labels to put on every resource.
    /// </summary>
    public IDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the resources the watchdog waits for, such as the namespace.
    /// </summary>
    public IList<Resource> DependsOn { get; init; } = [];
}
