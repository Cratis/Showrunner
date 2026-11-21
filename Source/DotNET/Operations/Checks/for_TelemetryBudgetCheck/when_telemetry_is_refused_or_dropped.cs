// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_telemetry_is_refused_or_dropped : a_telemetry_budget_check
{
    void Establish()
    {
        _prometheus.Vectors[CollectorQuery] = [FakePrometheusReader.Sample(3, ("signal", "otelcol_processor_refused_metric_points_total"), ("processor", "memory_limiter"))];
        _prometheus.Vectors[DeltaQuery] = [FakePrometheusReader.Sample(2, ("error", "stream_limit"))];
        _prometheus.Vectors[LokiQuery] = [FakePrometheusReader.Sample(1, ("reason", "rate_limited"))];
        _prometheus.Vectors[TempoQuery] = [FakePrometheusReader.Sample(1.5, ("reason", "trace_too_large"))];
    }

    async Task Because() => await Run();

    [Fact] void should_name_the_refused_signal_and_give_queue_advice() => Message("telemetry:collector:").ShouldContain("otelcol_processor_refused_metric_points_total");
    [Fact] void should_give_queue_advice_for_the_collector() => Message("telemetry:collector:").ShouldContain("queue");
    [Fact] void should_keep_the_error_label_of_delta_drops() => Message("telemetry:delta:").ShouldContain("\"error\":\"stream_limit\"");
    [Fact] void should_group_loki_drops_by_reason() => Message("telemetry:loki:").ShouldContain("\"reason\":\"rate_limited\"");
    [Fact] void should_group_tempo_drops_by_reason() => Message("telemetry:tempo:").ShouldContain("\"reason\":\"trace_too_large\"");
    [Fact] void should_show_the_value_and_the_threshold() => Message("telemetry:tempo:").ShouldContain("(" + "{\"reason\":\"trace_too_large\"}: 1.5; threshold 0)");
    [Fact] void should_keep_the_keys_free_of_the_counter_value() => Keys.ShouldContain("telemetry:collector:{\"processor\":\"memory_limiter\",\"signal\":\"otelcol_processor_refused_metric_points_total\"}");
    [Fact] void should_query_the_collector_counters_over_fifteen_minutes() =>
        _prometheus.Queries.First(query => query.Expression.Contains("receiver_refused")).Expression.ShouldContain("[15m:1m]");
}
#endif
