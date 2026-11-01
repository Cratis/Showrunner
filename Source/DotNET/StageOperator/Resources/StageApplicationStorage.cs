// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the storage of an application.</summary>
public record StageApplicationStorage
{
    /// <summary>Gets the size of the MongoDB volume.</summary>
    [JsonPropertyName("size")]
    public string? Size { get; init; }
}
