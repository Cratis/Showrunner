// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the usage of one UTC day.</summary>
public record StageApplicationPeriodUsage
{
    /// <summary>Gets the day as <c>yyyy-MM-dd</c>.</summary>
    [JsonPropertyName("day")]
    public string Day { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether any sample was taken that day.</summary>
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

    /// <summary>Gets the number of samples; kept so the average can keep accumulating.</summary>
    [JsonPropertyName("sampleCount")]
    public long SampleCount { get; init; }
}
