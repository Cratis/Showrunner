// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_health_metrics_are_missing_or_not_scraped : a_telemetry_budget_check
{
    IReadOnlyCollection<Alerts.Finding> _noSeries;
    IReadOnlyCollection<Alerts.Finding> _unscraped;

    async Task Because()
    {
        _prometheus.Vectors[SeriesQuery] = [];
        await Run();
        _noSeries = _findings;

        _prometheus.Vectors[SeriesQuery] = [FakePrometheusReader.Sample(1000)];
        _prometheus.Vectors[ScrapingQuery] =
        [
            FakePrometheusReader.Sample(1, ("service", "loki")),
            FakePrometheusReader.Sample(0, ("service", "tempo"))
        ];
        await Run();
        _unscraped = _findings;
    }

    [Fact] void should_not_treat_missing_head_series_as_healthy() => _noSeries.Single().Key.ShouldEqual("telemetry:api");
    [Fact] void should_explain_what_is_missing() => _noSeries.Single().Text.ShouldContain("series: required health metrics are missing");
    [Fact] void should_report_a_service_that_is_not_scraped() => _unscraped.Select(finding => finding.Key).ShouldContain("telemetry:scraping:otel-collector-telemetry");
    [Fact] void should_report_a_service_whose_target_is_down() => _unscraped.Select(finding => finding.Key).ShouldContain("telemetry:scraping:tempo");
    [Fact] void should_not_report_a_scraped_service() => _unscraped.Select(finding => finding.Key).ShouldNotContain("telemetry:scraping:loki");
}
#endif
