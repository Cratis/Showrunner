// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents a status condition.</summary>
public record StageApplicationCondition
{
    /// <summary>Gets the type: Ready, Progressing, Degraded or Expired.</summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>Gets the status: True or False.</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = "False";

    /// <summary>Gets the machine readable reason.</summary>
    [JsonPropertyName("reason")]
    public string Reason { get; init; } = string.Empty;

    /// <summary>Gets the human readable message.</summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets when the status last changed, as an RFC 3339 timestamp.</summary>
    [JsonPropertyName("lastTransitionTime")]
    public string LastTransitionTime { get; init; } = string.Empty;
}
