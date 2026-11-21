// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The Prometheus the <see cref="Watchdog"/> reads. Naming it turns on the <c>metrics</c> check (node filesystem and scrape targets)
/// and the <c>telemetry-budget</c> check (refused or dropped telemetry, series budget, container memory against its limit).
/// </summary>
public sealed class WatchdogPrometheusArgs
{
    /// <summary>
    /// Gets the Prometheus address inside the cluster, for example <c>http://prometheus-server.infrastructure:80</c>.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets the namespace the platform (node exporters, kube-state-metrics, the collector) runs in, or null for <c>infrastructure</c>.
    /// </summary>
    public string? PlatformNamespace { get; init; }

    /// <summary>
    /// Gets the number of head series above which Prometheus is over budget, or null for 350000.
    /// </summary>
    public int? SeriesThreshold { get; init; }
}
