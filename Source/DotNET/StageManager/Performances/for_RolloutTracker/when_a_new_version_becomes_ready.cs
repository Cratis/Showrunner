// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_a_new_version_becomes_ready : Specification
{
    PerformanceStatus _status;

    void Because()
    {
        _status = Releases.RunningV140();
        RolloutTracker.Observe(_status, Releases.V150, Releases.V150, false, Releases.Start, Releases.Timeout);
        RolloutTracker.Observe(_status, Releases.V150, Releases.V150, true, Releases.Start.AddMinutes(1), Releases.Timeout);
    }

    [Fact] void should_append_it_as_rolled_out() => _status.Versions[^1].Outcome.ShouldEqual(RolloutOutcome.RolledOut);
    [Fact] void should_append_the_new_version() => _status.Versions[^1].Version.ShouldEqual("1.5.0");
    [Fact] void should_keep_the_history() => _status.Versions.Count.ShouldEqual(2);
    [Fact] void should_run_the_new_version() => _status.RunningVersion.ShouldEqual("1.5.0");
    [Fact] void should_end_the_rollout() => _status.Rollout.ShouldBeNull();
}
#endif
