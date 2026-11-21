// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_a_node_has_no_metrics_yet : an_operational_metrics_check
{
    IReadOnlyCollection<Alerts.Finding> _settled;

    void Establish() => _reader.Nodes.Add(new NodeInfo("node-b", [new NodeCondition("Ready", "True")]));

    async Task Because()
    {
        await Run();
        _settled = _findings;
        _reader.Nodes.RemoveAt(1);
        _reader.Nodes.Add(new NodeInfo("node-b", [new NodeCondition("Ready", "True")]) { Created = _time.Now.AddMinutes(-1) });
        await Run();
    }

    [Fact] void should_require_the_metrics_of_a_settled_node() => _settled.Select(finding => finding.Key).ShouldContain("metrics:root-usage:missing:node-b");
    [Fact] void should_require_the_node_targets_of_a_settled_node() => _settled.Select(finding => finding.Key).ShouldContain("metrics:node-targets:missing:('kubernetes-nodes', 'node-b')");
    [Fact] void should_not_require_them_of_a_node_that_just_joined() => _findings.ShouldBeEmpty();
}
#endif
