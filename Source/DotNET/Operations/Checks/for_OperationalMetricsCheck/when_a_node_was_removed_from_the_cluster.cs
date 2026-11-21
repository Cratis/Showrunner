// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_a_node_was_removed_from_the_cluster : an_operational_metrics_check
{
    IReadOnlyCollection<Alerts.Finding> _whileRemoved;

    void Establish()
    {
        _prometheus.Vectors[NodeTargets] =
        [
            .. _prometheus.Vectors[NodeTargets],
            FakePrometheusReader.Sample(0, ("job", "kubernetes-nodes"), ("instance", "node-gone")),
            FakePrometheusReader.Sample(0, ("job", "kubernetes-nodes-cadvisor"), ("instance", "node-gone"))
        ];
        _prometheus.Vectors[RootUsage] = [.. _prometheus.Vectors[RootUsage], FakePrometheusReader.Sample(.9, ("node", "node-gone"))];
    }

    async Task Because()
    {
        await Run();
        _whileRemoved = _findings;
        _reader.Nodes.Add(new NodeInfo("node-gone", [new NodeCondition("Ready", "True")]));
        await Run();
    }

    [Fact] void should_not_report_the_leftover_series_of_a_node_that_is_gone() => _whileRemoved.ShouldBeEmpty();
    [Fact] void should_report_the_same_down_targets_of_a_node_that_exists() => Keys.Count(key => key.StartsWith("telemetry:target-down")).ShouldEqual(2);
}
#endif
