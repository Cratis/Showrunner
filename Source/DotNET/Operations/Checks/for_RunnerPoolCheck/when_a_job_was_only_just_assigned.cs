// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_RunnerPoolCheck;

public class when_a_job_was_only_just_assigned : a_runner_pool
{
    double _saved;

    void Establish() => _cluster.Runners.Add(Runner("late-job", job: "build", ageMinutes: 600));

    async Task Because()
    {
        await Run();
        _saved = Saved("late-job");
    }

    [Fact] void should_not_measure_from_the_age_of_the_pod() => _cluster.Deleted.ShouldBeEmpty();
    [Fact] void should_remember_when_the_job_was_first_seen() => _saved.ShouldEqual(_time.Now.ToUnixTimeSeconds());
    [Fact] void should_find_nothing() => _findings.ShouldBeEmpty();
}
#endif
