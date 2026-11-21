// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_RunnerPoolCheck;

public class when_more_runners_are_over_the_limit_than_one_pass_may_stop : a_runner_pool
{
    IReadOnlyCollection<Finding> _four;
    List<string> _deletedWithFour;
    List<string> _deletedWithThree;

    async Task Because()
    {
        foreach (var name in new[] { "a", "b", "c", "d" })
        {
            _cluster.Runners.Add(Runner(name, job: "x"));
            FirstSeen(name, 200);
        }

        await Run();
        _four = _findings;
        _deletedWithFour = [.. _cluster.Deleted];

        _cluster.Runners.RemoveAt(3);
        await Run();
        _deletedWithThree = [.. _cluster.Deleted];
    }

    [Fact] void should_stop_none_of_them() => _deletedWithFour.ShouldBeEmpty();
    [Fact] void should_fail_loudly() => _four.Single(finding => finding.Key == "runners:refused").Severity.ShouldEqual(AlertSeverity.Critical);
    [Fact] void should_say_how_many_were_refused() => _four.Single().Text.ShouldContain("4 runners are over the limit, more than the 3 one pass may stop");
    [Fact] void should_list_every_runner_over_the_limit() => _four.Single().Text.Split("job assigned for").Length.ShouldEqual(5);
    [Fact] void should_stop_exactly_the_cap() => _deletedWithThree.ShouldContainOnly("a", "b", "c");
}
#endif
