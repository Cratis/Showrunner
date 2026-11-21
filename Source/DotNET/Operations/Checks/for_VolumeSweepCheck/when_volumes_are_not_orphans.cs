// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_VolumeSweepCheck;

public class when_volumes_are_not_orphans : a_volume_sweep
{
    void Establish()
    {
        _cloud.Volumes.Remove(_cloud.Volumes.Single(volume => volume.Id == "orphan"));
        AddVolume("young", TimeSpan.FromMinutes(119));
        AddVolume("manual", Hours(5), title: "backup");
        AddVolume("foreign", Hours(5), cluster: "someone-else");
        AddVolume("unlabeled", Hours(5), cluster: null);
        AddVolume("exactly-old-enough", TimeSpan.FromHours(2));
    }

    async Task Because() => await Run();

    [Fact] void should_leave_a_volume_that_may_still_be_provisioning() => _cloud.Actions.ShouldNotContain("delete young");
    [Fact] void should_leave_a_volume_the_driver_did_not_name() => _cloud.Actions.ShouldNotContain("delete manual");
    [Fact] void should_leave_another_clusters_volume() => _cloud.Actions.ShouldNotContain("delete foreign");
    [Fact] void should_leave_a_volume_without_a_cluster_label() => _cloud.Actions.ShouldNotContain("delete unlabeled");
    [Fact] void should_delete_a_volume_that_is_exactly_old_enough() => _cloud.Actions.ShouldContain("delete exactly-old-enough");
    [Fact] void should_delete_only_that_one() => _cloud.Actions.Count(action => action.StartsWith("delete", StringComparison.Ordinal)).ShouldEqual(1);
}
#endif
