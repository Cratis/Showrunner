// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// CPU and memory a Performance used, accumulated from the metrics of its pods.
/// </summary>
public class ResourceUsageStatus
{
    /// <summary>
    /// Gets or sets the UTC day this covers, for a period. Empty for the accumulated total.
    /// </summary>
    [Description("The UTC day (yyyy-MM-dd) a period covers. Empty for the total.")]
    public string Day { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the metrics were available.
    /// </summary>
    [Description("Whether the cluster's metrics were available. False means the numbers are zero because nothing was measured.")]
    public bool Measured { get; set; }

    /// <summary>
    /// Gets or sets the CPU used, in core seconds.
    /// </summary>
    [Description("CPU used by all containers of the Performance, in core seconds.")]
    public double CpuCoreSeconds { get; set; }

    /// <summary>
    /// Gets or sets the highest memory seen at one time, in bytes.
    /// </summary>
    [Description("The highest memory of all containers together at one sample, in bytes.")]
    public double PeakMemoryBytes { get; set; }

    /// <summary>
    /// Gets or sets the average memory, in bytes.
    /// </summary>
    [Description("The average memory of all containers together over the samples, in bytes.")]
    public double AverageMemoryBytes { get; set; }

    /// <summary>
    /// Gets or sets when the first sample was taken.
    /// </summary>
    [Description("When the first sample was taken.")]
    public DateTimeOffset? WindowStart { get; set; }

    /// <summary>
    /// Gets or sets when the last sample was taken.
    /// </summary>
    [Description("When the last sample was taken.")]
    public DateTimeOffset? WindowEnd { get; set; }

    /// <summary>
    /// Gets or sets how many samples this holds.
    /// </summary>
    [Description("How many samples this holds.")]
    public long SampleCount { get; set; }
}
