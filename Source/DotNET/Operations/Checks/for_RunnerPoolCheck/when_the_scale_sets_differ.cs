// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_RunnerPoolCheck;

public class when_the_scale_sets_differ : a_runner_pool
{
    void Establish()
    {
        _options = _options with { ScaleSets = ["big", "small", "release"] };
        _cluster.Runners.AddRange(
        [
            Runner("elsewhere", scaleSet: "someone-else", job: "x"),
            Runner("small-hung", scaleSet: "small", job: "guard"),
            Runner("release-hung", scaleSet: "release", job: "publish")
        ]);
        FirstSeen("elsewhere", 500);
        FirstSeen("small-hung", 120);
        FirstSeen("release-hung", 120);
    }

    async Task Because() => await Run();

    [Fact] void should_ignore_the_runners_of_other_scale_sets() => _cluster.Deleted.ShouldNotContain("elsewhere");
    [Fact] void should_watch_the_small_scale_set() => _cluster.Deleted.ShouldContain("small-hung");
    [Fact] void should_watch_the_release_scale_set() => _cluster.Deleted.ShouldContain("release-hung");
    [Fact] void should_not_remember_the_runners_of_other_scale_sets() => SavedRunners().ShouldNotContain("elsewhere");
}
#endif
