// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_RolloutTracker;

public class when_the_history_grows_long : Specification
{
    PerformanceStatus _status;

    void Because()
    {
        _status = new();
        for (var version = 0; version < RolloutTracker.MaxVersions + 5; version++)
        {
            var release = new PublishedRelease($"1.{version}.0", "cratis/stage:5.3.0", $"crm-package-1-{version}-0");
            RolloutTracker.Observe(_status, release, release, true, Releases.Start.AddMinutes(version), Releases.Timeout);
        }
    }

    [Fact] void should_keep_the_newest_twenty() => _status.Versions.Count.ShouldEqual(RolloutTracker.MaxVersions);
    [Fact] void should_keep_the_newest_last() => _status.Versions[^1].Version.ShouldEqual("1.24.0");
    [Fact] void should_drop_the_oldest() => _status.Versions[0].Version.ShouldEqual("1.5.0");
}
#endif
