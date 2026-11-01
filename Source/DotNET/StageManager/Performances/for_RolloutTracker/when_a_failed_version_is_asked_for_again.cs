// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_a_failed_version_is_asked_for_again : Specification
{
    PerformanceStatus _status;
    PublishedRelease _deployed;

    void Because()
    {
        _status = Releases.V150FailedAndV140Restored();
        _deployed = RolloutTracker.ToDeploy(_status, Releases.V150);
        RolloutTracker.Observe(_status, _deployed, Releases.V150, true, Releases.Start.AddHours(1), Releases.Timeout);
    }

    [Fact] void should_keep_the_previous_version_running() => _deployed.ShouldEqual(Releases.V140);
    [Fact] void should_not_try_it_again() => _status.Versions.Count.ShouldEqual(3);
    [Fact] void should_keep_reporting_the_running_version() => _status.RunningVersion.ShouldEqual("1.4.0");
}
#endif
