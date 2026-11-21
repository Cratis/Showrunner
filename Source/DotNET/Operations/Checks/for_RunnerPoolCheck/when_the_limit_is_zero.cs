// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_RunnerPoolCheck;

public class when_the_limit_is_zero : a_runner_pool
{
    void Establish()
    {
        _options = _options with { MaximumJobTime = TimeSpan.Zero };
        _cluster.Runners.Add(Runner("hung", job: "x"));
        FirstSeen("hung", 999);
    }

    async Task Because() => await Run();

    [Fact] void should_report_and_stop_nothing() => _cluster.Deleted.ShouldBeEmpty();
    [Fact] void should_find_nothing() => _findings.ShouldBeEmpty();
    [Fact] void should_still_keep_track_of_the_job() => Saved("hung").ShouldEqual(_time.Now.ToUnixTimeSeconds() - (999 * 60));
}
#endif
