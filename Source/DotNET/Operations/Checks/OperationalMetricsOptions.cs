// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="OperationalMetricsCheck"/>.
/// </summary>
public sealed record OperationalMetricsOptions
{
    /// <summary>
    /// Gets the namespace the platform (node exporters, kube-state-metrics, the collector) runs in.
    /// </summary>
    public string PlatformNamespace { get; init; } = "infrastructure";

    /// <summary>
    /// Gets what a full root filesystem tells the reader to do.
    /// </summary>
    public string Advice { get; init; } = "expand the volume or free space on it";

    /// <summary>
    /// Gets the filesystem usage, as a fraction, above which a node is a finding.
    /// </summary>
    public double RootUsageThreshold { get; init; } = 0.85;
}
