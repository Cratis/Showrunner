// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the cpu and memory of a container.</summary>
public record StageApplicationResourceList
{
    /// <summary>Gets the cpu quantity.</summary>
    [JsonPropertyName("cpu")]
    public string? Cpu { get; init; }

    /// <summary>Gets the memory quantity.</summary>
    [JsonPropertyName("memory")]
    public string? Memory { get; init; }
}
