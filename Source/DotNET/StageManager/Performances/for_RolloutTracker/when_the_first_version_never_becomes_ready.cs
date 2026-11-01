// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_the_first_version_never_becomes_ready : Specification
{
    PerformanceStatus _status;
    PublishedRelease _deployed;

    void Because()
    {
        _status = new();
        RolloutTracker.Observe(_status, Releases.V140, Releases.V140, false, Releases.Start, Releases.Timeout);
        RolloutTracker.Observe(_status, Releases.V140, Releases.V140, false, Releases.Start.AddMinutes(6), Releases.Timeout);
        RolloutTracker.Observe(_status, Releases.V140, Releases.V140, false, Releases.Start.AddMinutes(7), Releases.Timeout);
        _deployed = RolloutTracker.ToDeploy(_status, Releases.V140);
    }

    [Fact] void should_record_one_failure() => _status.Versions.Single().Outcome.ShouldEqual(RolloutOutcome.Failed);
    [Fact] void should_have_nothing_to_roll_back_to_and_keep_deploying_it() => _deployed.ShouldEqual(Releases.V140);
    [Fact] void should_run_no_version() => _status.RunningVersion.ShouldEqual(string.Empty);
}
#endif
