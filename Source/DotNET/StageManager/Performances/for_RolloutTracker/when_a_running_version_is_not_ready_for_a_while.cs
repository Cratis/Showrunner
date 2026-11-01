// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_a_running_version_is_not_ready_for_a_while : Specification
{
    PerformanceStatus _status;

    void Because()
    {
        _status = Releases.RunningV140();
        RolloutTracker.Observe(_status, Releases.V140, Releases.V140, false, Releases.Start.AddHours(1), Releases.Timeout);
    }

    [Fact] void should_not_start_a_rollout() => _status.Rollout.ShouldBeNull();
    [Fact] void should_not_record_a_failure() => _status.Versions.Count.ShouldEqual(1);
}
#endif
