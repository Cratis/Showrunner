// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_a_failed_version_is_already_recorded_last : Specification
{
    PerformanceStatus _status;

    void Because()
    {
        _status = new();
        RolloutTracker.Observe(_status, Releases.V140, Releases.V140, false, Releases.Start, Releases.Timeout);
        RolloutTracker.Observe(_status, Releases.V140, Releases.V140, false, Releases.Start + Releases.Timeout, Releases.Timeout);
        RolloutTracker.Observe(_status, Releases.V140, Releases.V140, false, Releases.Start.AddHours(1), Releases.Timeout);
    }

    [Fact] void should_not_record_the_failure_twice() => _status.Versions.Count.ShouldEqual(1);
    [Fact] void should_not_roll_back() => _status.Versions.Any(record => record.Outcome == RolloutOutcome.RolledBack).ShouldBeFalse();
    [Fact] void should_run_no_version() => _status.RunningVersion.ShouldEqual(string.Empty);
}
#endif
