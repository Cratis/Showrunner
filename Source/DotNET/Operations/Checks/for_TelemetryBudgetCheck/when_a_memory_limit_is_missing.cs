// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_a_memory_limit_is_missing : a_telemetry_budget_check
{
    IReadOnlyCollection<Alerts.Finding> _noLimitsAtAll;
    IReadOnlyCollection<Alerts.Finding> _oneLimitMissing;

    async Task Because()
    {
        _prometheus.Vectors[MemoryQuery] = [Tempo(.95)];
        _prometheus.Vectors[LimitsQuery] = [];
        await Run();
        _noLimitsAtAll = _findings;

        _prometheus.Vectors[MemoryQuery] =
        [
            FakePrometheusReader.Sample(.95, ("namespace", "infrastructure"), ("pod", "decision-1"), ("container", "decision-engine")),
            FakePrometheusReader.Sample(.96, ("namespace", "infrastructure"), ("pod", "churned-1"), ("container", "worker"))
        ];
        _prometheus.Vectors[LimitsQuery] = [FakePrometheusReader.Sample(1073741824, ("namespace", "infrastructure"), ("pod", "decision-1"), ("container", "decision-engine"))];
        await Run();
        _oneLimitMissing = _findings;
    }

    [Fact] void should_be_unreadable_rather_than_a_healthy_skip() => _noLimitsAtAll.Select(finding => finding.Key).ShouldContain("telemetry:api");
    [Fact] void should_report_both_containers_when_one_limit_is_missing() => _oneLimitMissing.Count.ShouldEqual(2);
    [Fact] void should_give_the_bytes_of_the_container_with_a_limit() => _oneLimitMissing.Any(finding => finding.Text.Contains("decision-engine") && finding.Text.Contains("1024.0MiB")).ShouldBeTrue();
    [Fact] void should_still_report_the_container_without_a_limit() => _oneLimitMissing.Any(finding => finding.Text.Contains("churned-1/worker working set (96.0%)")).ShouldBeTrue();
}
#endif
