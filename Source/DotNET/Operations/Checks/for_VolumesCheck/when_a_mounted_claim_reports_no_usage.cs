// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_VolumesCheck;

public class when_a_mounted_claim_reports_no_usage : a_check
{
    protected override IWatchdogCheck Check => new VolumesCheck(_reader, new VolumesCheckOptions());

    void Establish()
    {
        _reader.Nodes.Add(new NodeInfo("n1", [new NodeCondition("Ready", "True")]));
        _reader.Nodes.Add(new NodeInfo("n2", [new NodeCondition("Ready", "False")]));
        _reader.Usage["n1"] = [];
        _reader.Usage["n2"] = [];
        var settled = _time.Now.AddHours(-1);
        _reader.Pods.Add(new PodInfo("shop", "db-0", "Running", settled, [], Started: settled, NodeName: "n1", Claims: ["missing"]));
        _reader.Pods.Add(new PodInfo("shop", "fresh-0", "Running", _time.Now.AddMinutes(-3), [], Started: _time.Now.AddMinutes(-3), NodeName: "n1", Claims: ["fresh"]));
        _reader.Pods.Add(new PodInfo("shop", "unready-0", "Running", settled, [], Started: settled, Ready: false, NodeName: "n1", Claims: ["unready"]));
        _reader.Pods.Add(new PodInfo("shop", "on-bad-node-0", "Running", settled, [], Started: settled, NodeName: "n2", Claims: ["bad-node"]));
        _reader.Claims.AddRange([
            new ClaimInfo("shop", "missing", "Bound"),
            new ClaimInfo("shop", "fresh", "Bound"),
            new ClaimInfo("shop", "unready", "Bound"),
            new ClaimInfo("shop", "bad-node", "Bound"),
            new ClaimInfo("shop", "unmounted", "Bound"),
            new ClaimInfo("shop", "pending", "Pending")]);
    }

    async Task Because() => await Run();

    [Fact] void should_report_a_bound_claim_that_a_settled_pod_mounts() => Keys.ShouldContain("pvc:shop/missing:usage-missing");
    [Fact] void should_give_a_recently_started_pod_ten_minutes_before_usage_is_required() => Keys.ShouldNotContain("pvc:shop/fresh:usage-missing");
    [Fact] void should_not_require_usage_from_a_pod_that_is_not_ready() => Keys.ShouldNotContain("pvc:shop/unready:usage-missing");
    [Fact] void should_not_require_usage_from_a_node_that_is_not_ready() => Keys.ShouldNotContain("pvc:shop/bad-node:usage-missing");
    [Fact] void should_not_require_usage_from_a_claim_nothing_mounts() => Keys.ShouldNotContain("pvc:shop/unmounted:usage-missing");
    [Fact] void should_not_require_usage_from_a_claim_that_is_not_bound() => Keys.ShouldNotContain("pvc:shop/pending:usage-missing");
}
#endif
