// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Channels;
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// Everything a watchdog needs to run, read from the environment by <see cref="WatchdogSettingsReader"/>.
/// </summary>
public sealed record WatchdogSettings
{
    /// <summary>
    /// Gets the timing and identity of the run.
    /// </summary>
    public required WatchdogOptions Options { get; init; }

    /// <summary>
    /// Gets the name of the ConfigMap the cooldown state is kept in.
    /// </summary>
    public string StateConfigMap { get; init; } = "cluster-health-watchdog-state";

    /// <summary>
    /// Gets the Discord webhook, or <see langword="null"/> when Discord is not a channel.
    /// </summary>
    public Uri? DiscordWebhook { get; init; }

    /// <summary>
    /// Gets the SMTP settings, or <see langword="null"/> when email is not a channel.
    /// </summary>
    public SmtpOptions? Smtp { get; init; }

    /// <summary>
    /// Gets the Cratis Direct settings, or <see langword="null"/> when Direct is not a channel.
    /// </summary>
    public DirectAlertsOptions? Direct { get; init; }

    /// <summary>
    /// Gets the Healthchecks URL, or <see langword="null"/> when there is no heartbeat.
    /// </summary>
    public Uri? Heartbeat { get; init; }

    /// <summary>
    /// Gets the names of the enabled checks.
    /// </summary>
    public IReadOnlyList<string> EnabledChecks { get; init; } = [];

    /// <summary>
    /// Gets the options of the volumes check.
    /// </summary>
    public VolumesCheckOptions Volumes { get; init; } = new();

    /// <summary>
    /// Gets the CronJobs that must keep succeeding.
    /// </summary>
    public IReadOnlyList<CronJobExpectation> CronJobs { get; init; } = [];

    /// <summary>
    /// Gets where the PBM backups live, or <see langword="null"/> when they are not watched.
    /// </summary>
    public PbmSettings? Pbm { get; init; }

    /// <summary>
    /// Gets the restore drill settings, or <see langword="null"/> when it is not watched.
    /// </summary>
    public RestoreDrillSettings? RestoreDrill { get; init; }

    /// <summary>
    /// Gets the workload whose readiness is probed, or <see langword="null"/> when none is.
    /// </summary>
    public ReadinessCheckOptions? Readiness { get; init; }

    /// <summary>
    /// Gets the public URLs to probe.
    /// </summary>
    public IReadOnlyList<Uri> PublicUrls { get; init; } = [];

    /// <summary>
    /// Gets the Prometheus base address, or <see langword="null"/> when Prometheus is not read.
    /// </summary>
    public Uri? Prometheus { get; init; }

    /// <summary>
    /// Gets the options of the operational metrics check.
    /// </summary>
    public OperationalMetricsOptions Metrics { get; init; } = new();

    /// <summary>
    /// Gets how Chronicle is read, or <see langword="null"/> when its observers are not watched.
    /// </summary>
    public ChronicleSettings? Chronicle { get; init; }

    /// <summary>
    /// Gets the options of the Chronicle observers check.
    /// </summary>
    public ChronicleObserversOptions ChronicleObservers { get; init; } = new();

    /// <summary>
    /// Gets the read-only GitHub token of the Actions audit, or <see langword="null"/> when the audit is not run. It is never logged.
    /// </summary>
    public string? GitHubAuditToken { get; init; }

    /// <summary>
    /// Gets how the runner pool is watched, or <see langword="null"/> when it is not.
    /// </summary>
    public RunnerPoolOptions? Runners { get; init; }

    /// <summary>
    /// Gets how the agent harness is watched, or <see langword="null"/> when it is not.
    /// </summary>
    public AgentHarnessOptions? AgentHarness { get; init; }

    /// <summary>
    /// Gets the options of the GitHub Actions schedule audit.
    /// </summary>
    public ActionsAuditOptions ActionsAudit { get; init; } = new();

    /// <summary>
    /// Gets the options of the telemetry budget check, which is enabled by naming it in <c>WATCHDOG_CHECKS</c>.
    /// </summary>
    public TelemetryBudgetOptions TelemetryBudget { get; init; } = new();
}
