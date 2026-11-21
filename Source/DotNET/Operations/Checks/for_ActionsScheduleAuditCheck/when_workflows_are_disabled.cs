// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_workflows_are_disabled : an_actions_audit
{
    void Establish()
    {
        Repositories("Cratis/off", "Cratis/mixed", "Cratis/archived");
        Workflows("Cratis/off", (1, "a.yml", "disabled_manually"));
        Workflows("Cratis/mixed", (2, "b.yml", "disabled_inactivity"), (3, "c.yml", "active"));
        ScheduledRuns("Cratis/mixed", (3, Hours(1)));
        WorkflowRuns("Cratis/mixed", 3, Hours(1));
        _github.Respond("/orgs/Cratis/repos?type=all&per_page=100", new object[]
        {
            new { full_name = "Cratis/off" },
            new { full_name = "Cratis/mixed" },
            new { full_name = "Cratis/archived", archived = true },
            new { full_name = "Cratis/forked", fork = true },
            new { full_name = "Cratis/dead", disabled = true }
        });
    }

    async Task Because() => await Run();

    [Fact] void should_report_each_disabled_workflow_with_identity_state_and_update_time() =>
        _findings.Single(finding => finding.Key == "actions-audit:disabled:Cratis/off:a.yml").Text
            .ShouldEqual("GitHub workflow Cratis/off/a.yml: disabled_manually; updated_at 2026-06-01T00:00:00Z; interval unknown");
    [Fact] void should_report_a_disabled_workflow_next_to_an_active_one() => Keys.ShouldContain("actions-audit:disabled:Cratis/mixed:b.yml");
    [Fact] void should_not_report_an_active_workflow_with_a_single_run() => Keys.Count().ShouldEqual(2);
    [Fact] void should_not_ask_for_runs_of_a_repository_with_nothing_active() => _github.Requests.ShouldNotContain("/repos/Cratis/off/actions/runs?event=schedule&per_page=100");
    [Fact] void should_never_look_at_archived_forked_or_disabled_repositories() => _github.Requests.Any(request => request.Contains("archived") || request.Contains("forked") || request.Contains("dead")).ShouldBeFalse();
}
#endif
