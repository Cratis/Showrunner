// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_the_deployed_release_is_not_the_target : Specification
{
    PerformanceStatus _status;

    void Because()
    {
        _status = Releases.RunningV140();
        _status.Rollout = new() { Version = "1.5.0", Image = "cratis/stage:5.3.0", PackageConfigMap = "crm-package-1-5-0", StartedAt = Releases.Start };
        RolloutTracker.Observe(_status, Releases.V140, Releases.V150, false, Releases.Start.AddHours(1), Releases.Timeout);
    }

    [Fact] void should_end_the_rollout() => _status.Rollout.ShouldBeNull();
    [Fact] void should_run_the_deployed_version() => _status.RunningVersion.ShouldEqual("1.4.0");
    [Fact] void should_record_nothing() => _status.Versions.Count.ShouldEqual(1);
}
#endif
