// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents one entry in the rollout history.</summary>
public record StageApplicationVersion
{
    /// <summary>Gets the version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the image.</summary>
    [JsonPropertyName("image")]
    public string Image { get; init; } = string.Empty;

    /// <summary>Gets the package ConfigMap, or null when the image carries the package.</summary>
    [JsonPropertyName("package")]
    public string? Package { get; init; }

    /// <summary>Gets when the rollout ended, as an RFC 3339 timestamp.</summary>
    [JsonPropertyName("rolledOutAt")]
    public string RolledOutAt { get; init; } = string.Empty;

    /// <summary>Gets the outcome: RolledOut, RolledBack or Failed.</summary>
    [JsonPropertyName("outcome")]
    public string Outcome { get; init; } = string.Empty;

    /// <summary>Gets why the rollout ended the way it did.</summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}
