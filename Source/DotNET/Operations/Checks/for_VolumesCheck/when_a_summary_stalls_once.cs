// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_VolumesCheck;

public class when_a_summary_stalls_once : a_check
{
    protected override IWatchdogCheck Check => new VolumesCheck(_reader, new VolumesCheckOptions());

    void Establish()
    {
        _reader.Nodes.Add(new NodeInfo("n1", [new NodeCondition("Ready", "True")]));
        _reader.Usage["n1"] = [new VolumeUsage("shop", "data", 100, 10)];
        _reader.SummaryFailuresBeforeSuccess = 1;
    }

    async Task Because() => await Run();

    [Fact] void should_retry_once_with_a_longer_timeout() => _reader.SummaryReads.Select(read => read.Timeout.TotalSeconds).ShouldContainOnly(5, 10);
    [Fact] void should_not_report_a_blip_that_clears_on_the_retry() => _findings.ShouldBeEmpty();
}
#endif
