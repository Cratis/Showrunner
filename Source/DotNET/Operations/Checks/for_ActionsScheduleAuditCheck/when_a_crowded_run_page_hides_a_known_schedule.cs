// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_a_crowded_run_page_hides_a_known_schedule : an_actions_audit
{
    List<string> _firstAudit;
    List<string> _secondAudit;
    string _remembered;

    void Establish()
    {
        Workflows("Cratis/app", Active(1, "known.yml"), Active(2, "unknown.yml"), (3, "off.yml", "disabled_manually"));
        ScheduledRuns("Cratis/app", (1, Hours(1)), (1, Hours(2)));
        WorkflowRuns("Cratis/app", 2, Hours(1));
    }

    async Task Because()
    {
        await Run();
        _firstAudit = [.. _github.Requests];
        _remembered = _audit.Persist()!;

        // Next day: the busy repository's page now only shows one run of the known schedule, and the unknown one is still absent.
        _github.Requests.Clear();
        _time.Advance(Hours(25));
        ScheduledRuns("Cratis/app", (1, Hours(1)));
        WorkflowRuns("Cratis/app", 1, Hours(1), Hours(2));
        await Run();
        _secondAudit = [.. _github.Requests];
    }

    [Fact] void should_remember_the_workflow_that_has_scheduled_runs() => _remembered.ShouldContain("Cratis/app/1");
    [Fact] void should_not_remember_a_workflow_that_never_showed_a_run() => _remembered.ShouldNotContain("Cratis/app/2");
    [Fact] void should_not_ask_for_the_history_of_a_workflow_it_has_never_seen_scheduled() => _firstAudit.Any(request => request.Contains("/workflows/")).ShouldBeFalse();
    [Fact] void should_ask_for_the_history_of_a_known_schedule_with_fewer_than_two_runs() => _secondAudit.ShouldContain("/repos/Cratis/app/actions/workflows/1/runs?event=schedule&per_page=10");
    [Fact] void should_not_fall_back_for_a_disabled_workflow() => _secondAudit.Any(request => request.Contains("/workflows/3/")).ShouldBeFalse();
}
#endif
