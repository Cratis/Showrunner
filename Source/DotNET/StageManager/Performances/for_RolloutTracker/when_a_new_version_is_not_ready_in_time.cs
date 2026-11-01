// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_a_new_version_is_not_ready_in_time : Specification
{
    PerformanceStatus _status;
    PublishedRelease _next;

    void Because()
    {
        _status = Releases.RunningV140();
        RolloutTracker.Observe(_status, Releases.V150, Releases.V150, false, Releases.Start, Releases.Timeout);
        RolloutTracker.Observe(_status, Releases.V150, Releases.V150, false, Releases.Start + Releases.Timeout, Releases.Timeout);
        _next = RolloutTracker.ToDeploy(_status, Releases.V150);
    }

    [Fact] void should_record_the_new_version_as_failed() => _status.Versions[1].Outcome.ShouldEqual(RolloutOutcome.Failed);
    [Fact] void should_say_why() => _status.Versions[1].Reason.ShouldContain("300 seconds");
    [Fact] void should_record_the_previous_version_as_rolled_back() => _status.Versions[2].Outcome.ShouldEqual(RolloutOutcome.RolledBack);
    [Fact] void should_name_the_restored_version() => _status.Versions[2].Version.ShouldEqual("1.4.0");
    [Fact] void should_run_the_previous_version() => _status.RunningVersion.ShouldEqual("1.4.0");
    [Fact] void should_end_the_rollout() => _status.Rollout.ShouldBeNull();
    [Fact] void should_deploy_the_previous_version_from_now_on() => _next.ShouldEqual(Releases.V140);
}
#endif
