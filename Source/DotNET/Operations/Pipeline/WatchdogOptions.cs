// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// The timing and identity of a watchdog run. The defaults are the ones the Cratis Infrastructure watchdog ran with.
/// </summary>
public sealed record WatchdogOptions
{
    /// <summary>
    /// Gets the namespace the watchdog runs in; its state ConfigMap and its own Jobs live there.
    /// </summary>
    public string Namespace { get; init; } = "default";

    /// <summary>
    /// Gets the name of the deployment shown in alert subjects. Defaults to the namespace.
    /// </summary>
    public string? Deployment { get; init; }

    /// <summary>
    /// Gets how long an alerted finding stays quiet before it is alerted again.
    /// </summary>
    public TimeSpan RealertInterval { get; init; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Gets how long a finding may stay open before it is escalated.
    /// </summary>
    public TimeSpan EscalateAfter { get; init; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets how often the watchdog is scheduled. When set, a gap of three intervals between runs is a finding.
    /// </summary>
    public TimeSpan? ScheduleInterval { get; init; }

    /// <summary>
    /// Gets the name of the CronJob that runs the watchdog. When set, a failed Job of it is reported once.
    /// </summary>
    public string? WatchdogCronJob { get; init; }

    /// <summary>
    /// Gets the name of the Job this run belongs to, so a crash is reported once for it.
    /// </summary>
    public string? WatchdogJobName { get; init; }

    /// <summary>
    /// Gets how long a run may take before Kubernetes kills it; it is the same number the Job's deadline carries.
    /// </summary>
    public TimeSpan Deadline { get; init; } = TimeSpan.FromSeconds(240);

    /// <summary>
    /// Gets the settle before a finding is looked at again: long enough for a dropped connection to have moved on.
    /// </summary>
    public TimeSpan RecheckPause { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Gets the room a re-check needs; below this much remaining the findings are alerted on as they stand.
    /// </summary>
    public TimeSpan RecheckBudget { get; init; } = TimeSpan.FromSeconds(90);

    /// <summary>
    /// Gets the time reserved at the end of a run for delivery, the heartbeat and the state write.
    /// </summary>
    public TimeSpan DeliveryBudget { get; init; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Gets the longest a single check may take.
    /// </summary>
    public TimeSpan CheckBudget { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets the name used in alert subjects.
    /// </summary>
    public string DeploymentName => Deployment ?? Namespace;
}
