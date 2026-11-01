// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_a_new_version_is_not_ready_yet : Specification
{
    PerformanceStatus _status;
    PublishedRelease _deployed;

    void Because()
    {
        _status = Releases.RunningV140();
        _deployed = RolloutTracker.ToDeploy(_status, Releases.V150);
        RolloutTracker.Observe(_status, _deployed, Releases.V150, false, Releases.Start, Releases.Timeout);
        RolloutTracker.Observe(_status, _deployed, Releases.V150, false, Releases.Start.AddMinutes(4), Releases.Timeout);
    }

    [Fact] void should_deploy_the_new_version() => _deployed.ShouldEqual(Releases.V150);
    [Fact] void should_wait_for_it_since_the_first_time_it_was_seen() => _status.Rollout.StartedAt.ShouldEqual(Releases.Start);
    [Fact] void should_name_the_version_being_rolled_out() => _status.Rollout.Version.ShouldEqual("1.5.0");
    [Fact] void should_keep_the_old_version_as_the_running_one() => _status.RunningVersion.ShouldEqual("1.4.0");
    [Fact] void should_record_nothing_yet() => _status.Versions.Count.ShouldEqual(1);
}
#endif
