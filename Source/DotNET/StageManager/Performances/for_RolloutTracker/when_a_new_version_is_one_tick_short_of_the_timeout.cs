// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_a_new_version_is_one_tick_short_of_the_timeout : Specification
{
    PerformanceStatus _status;

    void Because()
    {
        _status = Releases.RunningV140();
        RolloutTracker.Observe(_status, Releases.V150, Releases.V150, false, Releases.Start, Releases.Timeout);
        RolloutTracker.Observe(_status, Releases.V150, Releases.V150, false, Releases.Start + Releases.Timeout - TimeSpan.FromTicks(1), Releases.Timeout);
    }

    [Fact] void should_record_nothing_more() => _status.Versions.Count.ShouldEqual(1);
    [Fact] void should_keep_the_rollout_going() => _status.Rollout.ShouldNotBeNull();
    [Fact] void should_keep_running_the_stable_version() => _status.RunningVersion.ShouldEqual("1.4.0");
}
#endif
