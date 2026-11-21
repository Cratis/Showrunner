// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_the_node_list_cannot_be_read : an_operational_metrics_check
{
    void Establish()
    {
        _reader.Failing["ListNodes"] = null;
        _prometheus.Vectors[NodeTargets] = [FakePrometheusReader.Sample(0, ("job", "kubernetes-nodes"), ("instance", "node-x"))];
    }

    async Task Because() => await Run();

    [Fact] void should_report_the_unreadable_node_list() => Keys.ShouldContain("nodes:api");
    [Fact] void should_still_report_the_down_target() => Keys.ShouldContain("telemetry:target-down:kubernetes-nodes/node-x");
    [Fact] void should_not_claim_nodes_are_missing_from_metrics() => Keys.Any(key => key.StartsWith("metrics:root-usage:missing")).ShouldBeFalse();
}
#endif
