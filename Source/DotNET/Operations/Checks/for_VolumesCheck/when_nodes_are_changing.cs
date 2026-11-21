// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_VolumesCheck;

public class when_nodes_are_changing : a_check
{
    protected override IWatchdogCheck Check => new VolumesCheck(_reader, new VolumesCheckOptions());

    bool Read(string node) => _reader.SummaryReads.Any(read => read.Node == node);

    void Establish()
    {
        var ready = new NodeCondition("Ready", "True");
        var batch = new Dictionary<string, string> { ["workload"] = "batch" };
        _reader.Nodes.Add(new NodeInfo("new", [ready]) { Created = _time.Now.AddMinutes(-5) });
        _reader.Nodes.Add(new NodeInfo("leaving", [ready]) { TaintKeys = ["ToBeDeletedByClusterAutoscaler"] });
        _reader.Nodes.Add(new NodeInfo("deleting", [ready]) { Deleting = true });
        _reader.Nodes.Add(new NodeInfo("new-batch", [ready]) { Created = _time.Now.AddMinutes(-20), Labels = batch });
        _reader.Nodes.Add(new NodeInfo("settled-batch", [ready]) { Created = _time.Now.AddMinutes(-40), Labels = batch });
        _reader.Nodes.Add(new NodeInfo("settled", [ready]) { Created = _time.Now.AddMinutes(-11) });
    }

    async Task Because() => await Run();

    [Fact] void should_not_read_a_node_that_just_joined() => Read("new").ShouldBeFalse();
    [Fact] void should_not_read_a_node_that_is_leaving() => Read("leaving").ShouldBeFalse();
    [Fact] void should_not_read_a_node_that_is_being_deleted() => Read("deleting").ShouldBeFalse();
    [Fact] void should_give_a_batch_node_longer_to_settle() => Read("new-batch").ShouldBeFalse();
    [Fact] void should_report_a_settled_batch_node_under_the_batch_prefix() => Keys.ShouldContain("batch-node:settled-batch:summary");
    [Fact] void should_report_a_settled_regular_node_immediately() => Keys.ShouldContain("node:settled:summary");
    [Fact] void should_report_nothing_for_changing_nodes() => Keys.Count().ShouldEqual(2);
}
#endif
