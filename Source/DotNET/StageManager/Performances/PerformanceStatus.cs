// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The observed state of a <see cref="Performance"/>. It never contains a secret value.
/// </summary>
public class PerformanceStatus
{
    /// <summary>
    /// Gets or sets the phase.
    /// </summary>
    [Description("Pending, Blocked, Progressing, Ready or Expired.")]
    public PerformancePhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the public URL of the application, through AuthProxy.
    /// </summary>
    [Description("The public URL of the application, through AuthProxy.")]
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the environment keys the Performance needs and the environment does not have.
    /// </summary>
    [Description("The environment keys the Performance needs and the environment does not have. While this is not empty nothing is rolled out.")]
    public IList<string> MissingEnvironmentKeys { get; set; } = [];

    /// <summary>
    /// Gets or sets the generation of the spec these conditions describe.
    /// </summary>
    [Description("The generation of the spec these conditions describe.")]
    public long ObservedGeneration { get; set; }

    /// <summary>
    /// Gets or sets the state of the generated signing keys.
    /// </summary>
    [Description("The state of the generated signing keys.")]
    public SigningKeysStatus SigningKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets the conditions, one per component and one per precondition.
    /// </summary>
    [Description("One condition per component (Application, AuthProxy, Ante) and per precondition.")]
    public IList<ResourceCondition> Conditions { get; set; } = [];

    /// <summary>
    /// Gets or sets the version of the published application that is running.
    /// </summary>
    [Description("The version of the published application that is running. After a rollback this is not the version the spec asks for.")]
    public string RunningVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the Performance expires.
    /// </summary>
    [Description("When the Performance expires. Empty for a permanent one.")]
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets until when the data of an expired Performance is to be kept.
    /// </summary>
    [Description("Until when the data of an expired Performance is to be kept. The publisher removes the storage it provided after this.")]
    public DateTimeOffset? DataRetainedUntil { get; set; }

    /// <summary>
    /// Gets or sets the version history of a published application, the newest last.
    /// </summary>
    [Description("The versions of a published application, the newest last, at most twenty.")]
    public IList<RolloutRecord> Versions { get; set; } = [];

    /// <summary>
    /// Gets or sets the rollout that is under way.
    /// </summary>
    [Description("The rollout that is waiting to become ready.")]
    public RolloutProgress? Rollout { get; set; }

    /// <summary>
    /// Gets or sets the CPU and memory used since the Performance started to be measured.
    /// </summary>
    [Description("CPU and memory accumulated from the metrics of the pods of the Performance.")]
    public ResourceUsageStatus ResourceUsage { get; set; } = new();

    /// <summary>
    /// Gets or sets the usage of the UTC day that is under way.
    /// </summary>
    [Description("The usage of the current UTC day. It becomes lastPeriodUsage when the day ends.")]
    public ResourceUsageStatus CurrentPeriodUsage { get; set; } = new();

    /// <summary>
    /// Gets or sets the usage of the last completed UTC day.
    /// </summary>
    [Description("The usage of the last completed UTC day, for periodic reporting.")]
    public ResourceUsageStatus LastPeriodUsage { get; set; } = new();
}
