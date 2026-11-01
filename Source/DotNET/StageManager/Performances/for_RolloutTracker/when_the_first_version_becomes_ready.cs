// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_the_first_version_becomes_ready : Specification
{
    PerformanceStatus _status;
    PublishedRelease _deployed;

    void Because()
    {
        _status = new();
        _deployed = RolloutTracker.ToDeploy(_status, Releases.V140);
        RolloutTracker.Observe(_status, _deployed, Releases.V140, true, Releases.Start, Releases.Timeout);
    }

    [Fact] void should_deploy_the_version_asked_for() => _deployed.ShouldEqual(Releases.V140);
    [Fact] void should_record_it_as_rolled_out() => _status.Versions.Single().Outcome.ShouldEqual(RolloutOutcome.RolledOut);
    [Fact] void should_record_the_version() => _status.Versions.Single().Version.ShouldEqual("1.4.0");
    [Fact] void should_record_the_image() => _status.Versions.Single().Image.ShouldEqual("cratis/stage:5.3.0");
    [Fact] void should_record_the_package() => _status.Versions.Single().PackageConfigMap.ShouldEqual("crm-package-1-4-0");
    [Fact] void should_record_when() => _status.Versions.Single().RolledOutAt.ShouldEqual(Releases.Start);
    [Fact] void should_run_the_version() => _status.RunningVersion.ShouldEqual("1.4.0");
    [Fact] void should_have_no_rollout_under_way() => _status.Rollout.ShouldBeNull();
}
#endif
