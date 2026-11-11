// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_the_target_changes_during_a_rollout : Specification
{
    PerformanceStatus _status;
    PublishedRelease _newer;

    void Because()
    {
        _newer = new("1.6.0", "cratis/stage:5.3.0", "crm-package-1-6-0");
        _status = Releases.RunningV140();
        RolloutTracker.Observe(_status, Releases.V150, Releases.V150, false, Releases.Start, Releases.Timeout);
        RolloutTracker.Observe(_status, _newer, _newer, false, Releases.Start.AddMinutes(4), Releases.Timeout);
    }

    [Fact] void should_roll_out_the_new_target() => _status.Rollout!.Version.ShouldEqual("1.6.0");
    [Fact] void should_restart_the_clock() => _status.Rollout!.StartedAt.ShouldEqual(Releases.Start.AddMinutes(4));
    [Fact] void should_record_nothing() => _status.Versions.Count.ShouldEqual(1);
}
#endif
