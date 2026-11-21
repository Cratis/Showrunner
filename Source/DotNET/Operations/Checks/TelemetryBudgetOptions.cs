// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="TelemetryBudgetCheck"/>.
/// </summary>
public sealed record TelemetryBudgetOptions
{
    /// <summary>
    /// Gets the namespace whose container memory is watched, and where the telemetry services run.
    /// </summary>
    public string PlatformNamespace { get; init; } = "infrastructure";

    /// <summary>
    /// Gets the number of head series above which Prometheus is over budget. The comparison is strict.
    /// </summary>
    public int SeriesThreshold { get; init; } = 350000;

    /// <summary>
    /// Gets the containers that intentionally fill their cache and are not memory findings (WiredTiger).
    /// </summary>
    public IReadOnlyList<string> CacheDominatedContainers { get; init; } = ["mongod"];

    /// <summary>
    /// Gets the containers of the telemetry pipeline, whose memory findings carry ingest and queue advice.
    /// </summary>
    public IReadOnlyList<string> TelemetryContainers { get; init; } = ["otel-collector", "tempo", "loki", "prometheus-server", "prometheus"];

    /// <summary>
    /// Gets the Services that must be scraped for the budgets to mean anything.
    /// </summary>
    public IReadOnlyList<string> ScrapedServices { get; init; } = ["otel-collector-telemetry", "loki", "tempo"];
}
