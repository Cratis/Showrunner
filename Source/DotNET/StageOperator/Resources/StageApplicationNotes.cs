// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the Notes configuration of an application.</summary>
public record StageApplicationNotes
{
    /// <summary>Gets a value indicating whether Stage hosts the Notes backend.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <summary>Gets the optional name of a secret with extra environment for the Notes backend.</summary>
    [JsonPropertyName("configSecretRef")]
    public string? ConfigSecretRef { get; init; }
}
