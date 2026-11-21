// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_VolumesCheck;

public class when_volumes_fill_up : a_check
{
    const long Gi = 1L << 30;

    protected override IWatchdogCheck Check => new VolumesCheck(_reader, new VolumesCheckOptions { ThresholdPercent = 95, Advice = "see the storage runbook" });

    void Establish()
    {
        _reader.Nodes.Add(new NodeInfo("n1", [new NodeCondition("Ready", "True")]));
        _reader.Usage["n1"] = [
            new VolumeUsage("shop", "data-full", 100 * Gi, 90 * Gi),
            new VolumeUsage("shop", "data-edge", 100 * Gi, 85 * Gi),
            new VolumeUsage("shop", "data-fine", 100 * Gi, 10 * Gi)];
    }

    async Task Because() => await Run();

    [Fact] void should_report_90_percent_although_95_is_configured_because_the_threshold_is_capped_at_85() => Keys.ShouldContain("pvc:shop/data-full");
    [Fact] void should_describe_the_usage_and_the_advice() => _findings.First(finding => finding.Key == "pvc:shop/data-full").Text.ShouldEqual("volume shop/data-full at 90.0% (90Gi of 100Gi); see the storage runbook");
    [Fact] void should_not_report_a_volume_at_exactly_the_threshold() => Keys.ShouldNotContain("pvc:shop/data-edge");
    [Fact] void should_not_report_a_volume_with_room() => Keys.ShouldNotContain("pvc:shop/data-fine");
}
#endif
