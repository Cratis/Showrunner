// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the observed state of a published Stage application. Only the operator writes it.</summary>
public record StageApplicationStatus
{
    /// <summary>Gets the lifecycle phase.</summary>
    [JsonPropertyName("phase")]
    public string Phase { get; init; } = StageApplicationPhases.Pending;

    /// <summary>Gets the version that is running.</summary>
    [JsonPropertyName("runningVersion")]
    public string? RunningVersion { get; init; }

    /// <summary>Gets the image that is running.</summary>
    [JsonPropertyName("runningImage")]
    public string? RunningImage { get; init; }

    /// <summary>Gets the public URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the expiry as an RFC 3339 timestamp, or empty when permanent.</summary>
    [JsonPropertyName("expiresAt")]
    public string? ExpiresAt { get; init; }

    /// <summary>Gets the time until which the data of a stopped application is retained.</summary>
    [JsonPropertyName("dataRetainedUntil")]
    public string? DataRetainedUntil { get; init; }

    /// <summary>Gets the generation of the resource the status describes.</summary>
    [JsonPropertyName("observedGeneration")]
    public long ObservedGeneration { get; init; }

    /// <summary>Gets the conditions.</summary>
    [JsonPropertyName("conditions")]
    public IReadOnlyList<StageApplicationCondition> Conditions { get; init; } = [];

    /// <summary>Gets the rollout history, newest last.</summary>
    [JsonPropertyName("versions")]
    public IReadOnlyList<StageApplicationVersion> Versions { get; init; } = [];

    /// <summary>Gets the rollout that is in progress, if any.</summary>
    [JsonPropertyName("rollout")]
    public StageApplicationRollout? Rollout { get; init; }

    /// <summary>Gets the usage accumulated since the application was first measured.</summary>
    [JsonPropertyName("resourceUsage")]
    public StageApplicationUsage? ResourceUsage { get; init; }

    /// <summary>Gets the usage of the last completed UTC day.</summary>
    [JsonPropertyName("lastPeriodUsage")]
    public StageApplicationPeriodUsage? LastPeriodUsage { get; init; }

    /// <summary>Gets the usage of the UTC day in progress.</summary>
    [JsonPropertyName("currentDayUsage")]
    public StageApplicationPeriodUsage? CurrentDayUsage { get; init; }
}
