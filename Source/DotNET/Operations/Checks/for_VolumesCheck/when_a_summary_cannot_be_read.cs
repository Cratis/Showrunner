// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_VolumesCheck;

public class when_a_summary_cannot_be_read : a_check
{
    protected override IWatchdogCheck Check => new VolumesCheck(_reader, new VolumesCheckOptions());

    void Establish()
    {
        _reader.Nodes.Add(new NodeInfo("n1", [new NodeCondition("Ready", "True")]));
        _reader.Pods.Add(new PodInfo("shop", "db-0", "Running", _time.Now.AddDays(-1), [], Started: _time.Now.AddDays(-1), NodeName: "n1", Claims: ["data"]));
        _reader.Claims.Add(new ClaimInfo("shop", "data", "Bound"));
    }

    async Task Because() => await Run();

    [Fact] void should_report_the_node_whose_summary_cannot_be_read() => Keys.ShouldContain("node:n1:summary");
    [Fact] void should_have_tried_a_second_time_before_giving_up() => _reader.SummaryReads.Count.ShouldEqual(2);
    [Fact] void should_not_blame_the_claims_on_a_node_it_could_not_read() => Keys.ShouldNotContain("pvc:shop/data:usage-missing");
}
#endif
