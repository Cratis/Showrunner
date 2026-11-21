// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// The telemetry budget check against a Prometheus whose budgets are all healthy; a specification spoils what it needs to.
/// </summary>
public abstract class a_telemetry_budget_check : a_check
{
    protected const string CollectorQuery = "otelcol_(receiver_refused";
    protected const string DeltaQuery = "deltatocumulative";
    protected const string LokiQuery = "loki_discarded";
    protected const string TempoQuery = "tempo_discarded";
    protected const string ScrapeQuery = "prometheus_target_scrapes_exceeded";
    protected const string SeriesQuery = "prometheus_tsdb_head_series";
    protected const string MemoryQuery = "container_memory_working_set_bytes";
    protected const string LimitsQuery = "max by (namespace, pod, container) (kube_pod_container_resource_limits";
    protected const string ScrapingQuery = "count by (service)";

    protected FakePrometheusReader _prometheus;
    protected TelemetryBudgetOptions _options = new();

    protected override IWatchdogCheck Check => new TelemetryBudgetCheck(_prometheus, _options);

    protected static PrometheusSample Tempo(double value) => FakePrometheusReader.Sample(value, ("namespace", "infrastructure"), ("pod", "tempo-1"), ("container", "tempo"));

    protected string Message(string keyStart) => _findings.Single(finding => finding.Key.StartsWith(keyStart, StringComparison.Ordinal)).Text;

    void Establish()
    {
        _prometheus = new FakePrometheusReader();

        // The memory query contains the limits query, so it must be matched first.
        _prometheus.Vectors[MemoryQuery] = [Tempo(.5)];
        _prometheus.Vectors[LimitsQuery] = [Tempo(1073741824)];
        _prometheus.Vectors[SeriesQuery] = [FakePrometheusReader.Sample(1000)];
        _prometheus.Vectors[ScrapingQuery] =
        [
            FakePrometheusReader.Sample(1, ("service", "otel-collector-telemetry")),
            FakePrometheusReader.Sample(1, ("service", "loki")),
            FakePrometheusReader.Sample(1, ("service", "tempo"))
        ];
    }
}
#endif
