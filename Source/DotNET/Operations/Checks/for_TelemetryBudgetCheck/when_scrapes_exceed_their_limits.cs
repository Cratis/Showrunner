// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_scrapes_exceed_their_limits : a_telemetry_budget_check
{
    IReadOnlyCollection<Alerts.Finding> _attributed;
    IReadOnlyCollection<Alerts.Finding> _recovered;
    IReadOnlyCollection<Alerts.Finding> _unreadable;

    async Task Because()
    {
        _prometheus.Vectors[ScrapeQuery] = [FakePrometheusReader.Sample(2, ("limit_counter", "sample"))];
        _prometheus.Targets.Add(new ScrapeTarget("kubernetes-pods", "collector:8889", "down", "sample limit exceeded"));
        _prometheus.Targets.Add(new ScrapeTarget("kubernetes-pods", "other:9100", "down", "connection refused"));
        await Run();
        _attributed = _findings;

        _prometheus.Targets.Clear();
        await Run();
        _recovered = _findings;

        _prometheus.TargetsFail = true;
        await Run();
        _unreadable = _findings;
    }

    [Fact] void should_name_the_target_that_failed() => _attributed.Select(finding => finding.Key).ShouldContain("telemetry:scrape-target:kubernetes-pods/collector:8889");
    [Fact] void should_not_name_a_target_that_is_down_for_another_reason() => _attributed.Select(finding => finding.Key).ShouldNotContain("telemetry:scrape-target:kubernetes-pods/other:9100");
    [Fact] void should_say_all_app_metrics_were_lost_for_the_collector_target() => _attributed.Single(finding => finding.Key.StartsWith("telemetry:scrape-target:")).Text.ShouldContain("all app metrics were lost");
    [Fact] void should_attribute_the_counter_to_the_failing_targets() => _attributed.Single(finding => finding.Key.StartsWith("telemetry:scrape:")).Text.ShouldContain("current limit-failing targets: kubernetes-pods/collector:8889");
    [Fact] void should_not_blame_the_metrics_server_for_a_recovered_target() => _recovered.Single().Text.ShouldContain("offending target already recovered or could not be attributed");
    [Fact] void should_treat_an_unreadable_target_list_as_unhealthy() => _unreadable.Single(finding => finding.Key == "telemetry:api").Text.ShouldContain("scrape-targets:");
}
#endif
