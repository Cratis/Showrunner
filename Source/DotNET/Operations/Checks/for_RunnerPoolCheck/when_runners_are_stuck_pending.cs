// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_RunnerPoolCheck;

public class when_runners_are_stuck_pending : a_runner_pool
{
    IReadOnlyCollection<Finding> _one;
    IReadOnlyCollection<Finding> _five;
    List<string> _deletedWithFive;

    async Task Because()
    {
        _cluster.Runners.AddRange([Runner("stuck", ageMinutes: 45, phase: "Pending"), Runner("starting", ageMinutes: 2, phase: "Pending")]);
        await Run();
        _one = _findings;

        _cluster.Deleted.Clear();
        _cluster.Runners.Clear();
        _cluster.Runners.AddRange(new[] { "a", "b", "c", "d", "e" }.Select(name => Runner(name, ageMinutes: 45, phase: "Pending")));
        await Run();
        _five = _findings;
        _deletedWithFive = [.. _cluster.Deleted];
    }

    [Fact] void should_replace_a_prolonged_pending_runner_without_a_job() => _one.Select(finding => finding.Key).ShouldContainOnly("runners:replaced:stuck");
    [Fact] void should_say_how_long_it_was_pending() => _one.Single().Text.ShouldContain("stuck (pending 45 min)");
    [Fact] void should_not_alarm_for_a_replacement() => _one.Single().Severity.ShouldEqual(AlertSeverity.Information);
    [Fact] void should_leave_a_runner_that_is_only_starting() => _one.Select(finding => finding.Text).ShouldNotContain(text => text.Contains("starting"));
    [Fact] void should_replace_no_more_than_the_cap_per_pass() => _deletedWithFive.Count.ShouldEqual(3);
    [Fact] void should_report_the_ones_left_for_the_next_pass() => _five.Count(finding => finding.Key.StartsWith("runners:pending:", StringComparison.Ordinal)).ShouldEqual(2);
}
#endif
