// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_something_else_is_asked_for_after_a_failure : Specification
{
    PublishedRelease _fixed;
    PublishedRelease _deployed;
    PerformanceStatus _status;

    void Because()
    {
        _fixed = Releases.V150 with { PackageConfigMap = "crm-package-1-5-0-fixed" };
        _status = Releases.V150FailedAndV140Restored();
        _deployed = RolloutTracker.ToDeploy(_status, _fixed);
    }

    [Fact] void should_deploy_it() => _deployed.ShouldEqual(_fixed);
}
#endif
