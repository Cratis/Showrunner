// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_the_target_is_the_stable_version_but_not_ready : Specification
{
    PerformanceStatus _status;

    void Because()
    {
        _status = Releases.RunningV140();
        RolloutTracker.Observe(_status, Releases.V140, Releases.V140, false, Releases.Start.AddDays(1), Releases.Timeout);
    }

    [Fact] void should_record_nothing() => _status.Versions.Count.ShouldEqual(1);
    [Fact] void should_not_start_a_rollout() => _status.Rollout.ShouldBeNull();
    [Fact] void should_run_the_stable_version() => _status.RunningVersion.ShouldEqual("1.4.0");
}
#endif
