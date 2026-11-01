// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the usage accumulated over a window.</summary>
public record StageApplicationUsage
{
    /// <summary>Gets a value indicating whether any sample was taken.</summary>
    [JsonPropertyName("measured")]
    public bool Measured { get; init; }

    /// <summary>Gets the cpu used in core-seconds.</summary>
    [JsonPropertyName("cpuCoreSeconds")]
    public double CpuCoreSeconds { get; init; }

    /// <summary>Gets the highest memory use in bytes.</summary>
    [JsonPropertyName("peakMemoryBytes")]
    public long PeakMemoryBytes { get; init; }

    /// <summary>Gets the average memory use in bytes.</summary>
    [JsonPropertyName("averageMemoryBytes")]
    public long AverageMemoryBytes { get; init; }

    /// <summary>Gets the start of the window, as an RFC 3339 timestamp.</summary>
    [JsonPropertyName("windowStart")]
    public string? WindowStart { get; init; }

    /// <summary>Gets the end of the window, as an RFC 3339 timestamp.</summary>
    [JsonPropertyName("windowEnd")]
    public string? WindowEnd { get; init; }

    /// <summary>Gets the number of samples.</summary>
    [JsonPropertyName("sampleCount")]
    public long SampleCount { get; init; }
}
