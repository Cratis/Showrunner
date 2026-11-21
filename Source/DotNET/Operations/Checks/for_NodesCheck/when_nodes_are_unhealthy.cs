// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_NodesCheck;

public class when_nodes_are_unhealthy : a_check
{
    protected override IWatchdogCheck Check => new NodesCheck(_reader);

    void Establish()
    {
        _reader.Nodes.Add(new NodeInfo("down", [new NodeCondition("Ready", "False", "KubeletNotReady")]));
        _reader.Nodes.Add(new NodeInfo("unknown", [new NodeCondition("Ready", "Unknown")]));
        _reader.Nodes.Add(new NodeInfo("pressured", [
            new NodeCondition("Ready", "True"),
            new NodeCondition("MemoryPressure", "True"),
            new NodeCondition("DiskPressure", "True"),
            new NodeCondition("PIDPressure", "True")]));
        _reader.Nodes.Add(new NodeInfo("calm", [
            new NodeCondition("Ready", "True"),
            new NodeCondition("MemoryPressure", "False"),
            new NodeCondition("DiskPressure", "Unknown")]));
    }

    async Task Because() => await Run();

    [Fact] void should_report_a_node_that_is_not_ready() => Keys.ShouldContain("node:down:NotReady");
    [Fact] void should_say_why_it_is_not_ready() => _findings.First(finding => finding.Key == "node:down:NotReady").Text.ShouldContain("status: False, reason: KubeletNotReady");
    [Fact] void should_report_a_node_whose_readiness_is_unknown() => Keys.ShouldContain("node:unknown:NotReady");
    [Fact] void should_report_every_kind_of_pressure() => Keys.Where(key => key.StartsWith("node:pressured:", StringComparison.Ordinal)).ShouldContainOnly("node:pressured:MemoryPressure", "node:pressured:DiskPressure", "node:pressured:PIDPressure");
    [Fact] void should_not_alert_on_false_or_unknown_pressure() => Keys.Any(key => key.StartsWith("node:calm:", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
