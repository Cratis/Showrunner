// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_the_schedule_cache_changes : an_actions_audit
{
    string[] _afterTheFirstAudit;
    string[] _afterAWorkflowWasRemoved;
    string[] _bounded;
    string[] _boundedWithASkippedRepository;

    string[] Remembered() => [.. ((JsonArray)JsonNode.Parse(_audit.Persist()!)!["scheduled"]!).Select(key => key!.GetValue<string>())];

    async Task Because()
    {
        Workflows("Cratis/app", Active(1, "a.yml"), Active(2, "b.yml"));
        ScheduledRuns("Cratis/app", (1, Hours(1)), (1, Hours(2)), (2, Hours(1)), (2, Hours(3)));
        await Run();
        _afterTheFirstAudit = Remembered();

        // The next day workflow 2 is gone from the repository.
        _time.Advance(Hours(25));
        Workflows("Cratis/app", Active(1, "a.yml"));
        ScheduledRuns("Cratis/app", (1, Hours(1)), (1, Hours(2)));
        await Run();
        _afterAWorkflowWasRemoved = Remembered();

        // The cache holds at most the configured number of schedules.
        _audit = null;
        _options = new ActionsAuditOptions { MaximumRememberedSchedules = 2 };
        Workflows("Cratis/app", Active(1, "a.yml"), Active(2, "b.yml"), Active(3, "c.yml"));
        ScheduledRuns("Cratis/app", (1, Hours(1)), (1, Hours(2)), (2, Hours(1)), (2, Hours(2)), (3, Hours(1)), (3, Hours(2)));
        await Run();
        _bounded = Remembered();

        // A repository that cannot be read keeps its schedules even when the bound would drop them.
        _audit = null;
        Repositories("Cratis/app", "Cratis/hidden");
        _audit = new Checks.ActionsScheduleAuditCheck(_github, _options);
        _audit.Restore("{\"scheduled\":[\"Cratis/hidden/9\",\"Cratis/app/1\",\"Cratis/app/2\",\"Cratis/app/3\"]}");
        await Run();
        _boundedWithASkippedRepository = Remembered();
    }

    [Fact] void should_remember_the_workflows_that_have_scheduled_runs() => _afterTheFirstAudit.ShouldContainOnly("Cratis/app/1", "Cratis/app/2");
    [Fact] void should_forget_a_workflow_that_was_removed() => _afterAWorkflowWasRemoved.ShouldContainOnly("Cratis/app/1");
    [Fact] void should_bound_the_cache() => _bounded.Length.ShouldEqual(2);
    [Fact] void should_keep_the_schedules_of_a_skipped_repository_first() => _boundedWithASkippedRepository.First().ShouldEqual("Cratis/hidden/9");
    [Fact] void should_still_apply_the_bound_with_a_skipped_repository() => _boundedWithASkippedRepository.Length.ShouldEqual(2);
}
#endif
