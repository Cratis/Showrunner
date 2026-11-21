// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_RunnerPoolCheck;

public class when_a_job_runs_past_the_limit : a_runner_pool
{
    List<string> _stateWhenDeleting = [];
    string[] _savedAfterwards;

    void Establish()
    {
        _cluster.Runners.AddRange([Runner("hung", job: "build"), Runner("fine", job: "specs"), Runner("warm-idle", ageMinutes: 600), Runner("at", job: "a"), Runner("short", job: "b")]);
        FirstSeen("hung", 91);
        FirstSeen("fine", 10);
        FirstSeen("at", 90);
        FirstSeen("short", 89);
        FirstSeen("done", 60);
        _cluster.BeforeDelete = () => _stateWhenDeleting.Add(_store.Writes.Count.ToString());
    }

    async Task Because()
    {
        await Run();
        _savedAfterwards = SavedRunners();
    }

    [Fact] void should_stop_the_job_over_the_limit_and_the_one_exactly_at_it() => _cluster.Deleted.ShouldContainOnly("hung", "at");
    [Fact] void should_leave_the_job_one_minute_short_alone() => _cluster.Deleted.ShouldNotContain("short");
    [Fact] void should_never_stop_an_idle_runner_however_old_it_is() => _cluster.Deleted.ShouldNotContain("warm-idle");
    [Fact] void should_say_which_job_was_stopped_and_for_how_long() =>
        _findings.Single(finding => finding.Key == "runners:stopped:hung").Text.ShouldContain("hung (Cratis/Studio, build; job assigned for 91 min)");
    [Fact] void should_save_the_state_before_anything_is_stopped() => _stateWhenDeleting.ShouldContainOnly("1", "1");
    [Fact] void should_keep_only_the_runners_that_still_hold_a_job() => _savedAfterwards.ShouldContainOnly("hung", "fine", "at", "short");
}
#endif
