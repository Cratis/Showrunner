// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_VolumesCheck;

public class when_nodes_and_claims_cannot_be_listed : a_check
{
    protected override IWatchdogCheck Check => new VolumesCheck(_reader, new VolumesCheckOptions());

    void Establish()
    {
        _reader.Failing[nameof(IClusterReader.ListNodes)] = 403;
        _reader.Failing[nameof(IClusterReader.ListClaims)] = 403;
        var settled = _time.Now.AddHours(-1);
        _reader.Pods.Add(new PodInfo("shop", "db-0", "Running", settled, [], Started: settled, NodeName: "n1", Claims: ["data"]));
    }

    async Task Because() => await Run();

    [Fact] void should_report_the_unreadable_node_list() => Keys.ShouldContain("nodes:api");
    [Fact] void should_not_flood_with_missing_usage_when_there_is_nothing_to_compare_against() => Keys.Any(key => key.EndsWith(":usage-missing", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
