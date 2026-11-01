// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents a rollout in progress.</summary>
public record StageApplicationRollout
{
    /// <summary>Gets the version being rolled out.</summary>
    [JsonPropertyName("targetVersion")]
    public string TargetVersion { get; init; } = string.Empty;

    /// <summary>Gets the image being rolled out.</summary>
    [JsonPropertyName("targetImage")]
    public string TargetImage { get; init; } = string.Empty;

    /// <summary>Gets the package ConfigMap being rolled out, or null when the image carries the package.</summary>
    [JsonPropertyName("targetPackage")]
    public string? TargetPackage { get; init; }

    /// <summary>Gets when the rollout started, as an RFC 3339 timestamp.</summary>
    [JsonPropertyName("startedAt")]
    public string StartedAt { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether this rollout restores the previous image.</summary>
    [JsonPropertyName("isRollback")]
    public bool IsRollback { get; init; }
}
