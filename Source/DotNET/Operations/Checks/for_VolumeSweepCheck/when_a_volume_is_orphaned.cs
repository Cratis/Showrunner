// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_VolumeSweepCheck;

public class when_a_volume_is_orphaned : a_volume_sweep
{
    void Establish()
    {
        _cloud.Attached["orphan"] = [new CloudAttachment("server-1", true, "0:2")];
        _cloud.AttachedForAttempts = 2;
    }

    async Task Because() => await Run();

    [Fact] void should_detach_before_deleting() => _cloud.Actions.First().ShouldEqual("detach server-1@0:2");
    [Fact] void should_keep_trying_while_the_cloud_says_it_is_still_attached() => _cloud.Actions.Count(action => action == "delete orphan").ShouldEqual(3);
    [Fact] void should_never_touch_the_volume_a_persistent_volume_uses() => _cloud.Actions.ShouldNotContain("delete live");
    [Fact] void should_say_what_it_deleted() => _findings.Single().Text.ShouldEqual("volume sweeper deleted 1 orphaned volume(s): orphan (10 GB)");
    [Fact] void should_not_alarm_for_a_cleanup() => _findings.Single().Severity.ShouldEqual(AlertSeverity.Information);
}
#endif
