// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_the_audit_has_run_today : an_actions_audit
{
    IReadOnlyCollection<Alerts.Finding> _first;
    IReadOnlyCollection<Alerts.Finding> _sameDay;
    int _requestsSameDay;
    IReadOnlyCollection<Alerts.Finding> _nextDay;
    IReadOnlyCollection<Alerts.Finding> _restoredElsewhere;
    int _requestsRestored;

    async Task Because()
    {
        Workflows("Cratis/app", (1, "a.yml", "disabled_manually"));
        await Run();
        _first = _findings;
        var saved = _audit.Persist();

        _github.Requests.Clear();
        _time.Advance(Hours(5));
        await Run();
        _sameDay = _findings;
        _requestsSameDay = _github.Requests.Count;

        // The state lives in the ConfigMap, so a later run, with a new check, repeats the findings without asking GitHub.
        _audit = new ActionsScheduleAuditCheck(_github, _options);
        _audit.Restore(saved);
        _github.Requests.Clear();
        await Run();
        _restoredElsewhere = _findings;
        _requestsRestored = _github.Requests.Count;

        // A day later the problem was fixed, and the audit sees it clear.
        _time.Advance(Hours(20));
        Workflows("Cratis/app", Active(1, "a.yml"));
        ScheduledRuns("Cratis/app", (1, Hours(1)), (1, Hours(2)));
        await Run();
        _nextDay = _findings;
    }

    [Fact] void should_find_the_problem() => _first.Select(finding => finding.Key).ShouldContainOnly("actions-audit:disabled:Cratis/app:a.yml");
    [Fact] void should_repeat_the_findings_the_same_day_so_they_keep_their_open_since_time() => _sameDay.ShouldEqual(_first);
    [Fact] void should_not_ask_github_again_the_same_day() => _requestsSameDay.ShouldEqual(0);
    [Fact] void should_repeat_the_findings_from_restored_state() => _restoredElsewhere.Select(finding => finding.Key).ShouldContainOnly("actions-audit:disabled:Cratis/app:a.yml");
    [Fact] void should_not_ask_github_when_the_state_is_restored() => _requestsRestored.ShouldEqual(0);
    [Fact] void should_clear_a_fixed_problem_after_a_day() => _nextDay.ShouldBeEmpty();
}
#endif
