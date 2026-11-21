// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_every_platform_target_is_down : an_operational_metrics_check
{
    void Establish()
    {
        foreach (var key in new[] { NodeTargets, ServiceTargets, CollectorTargets })
        {
            _prometheus.Vectors[key] = [.. _prometheus.Vectors[key].Select(sample => new PrometheusSample(sample.Labels, 0))];
        }
    }

    async Task Because() => await Run();

    [Fact] void should_name_each_down_target() => Keys.Count(key => key.StartsWith("telemetry:target-down")).ShouldEqual(4);
    [Fact] void should_name_kube_state_metrics() => _findings.Any(finding => finding.Text.Contains("kube-state-metrics")).ShouldBeTrue();
    [Fact] void should_name_the_collector() => Keys.ShouldContain("telemetry:target-down:kubernetes-pods/collector:8889");
}
#endif
