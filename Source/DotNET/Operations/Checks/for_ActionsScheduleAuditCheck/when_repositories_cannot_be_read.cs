// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_repositories_cannot_be_read : an_actions_audit
{
    IReadOnlyCollection<Alerts.Finding> _others;
    IReadOnlyCollection<Alerts.Finding> _keptWhileHidden;
    IReadOnlyCollection<Alerts.Finding> _afterItBecameAccessible;
    IReadOnlyCollection<Alerts.Finding> _allHidden;
    string _stateAfterAllHidden;

    async Task Because()
    {
        Repositories("Cratis/secret", "Cratis/forbidden", "Cratis/app");
        _github.Fail("/repos/Cratis/forbidden/actions/workflows?per_page=100", new GitHubApiException(403));
        Workflows("Cratis/app", (1, "x.yml", "disabled_manually"));
        _audit = new ActionsScheduleAuditCheck(_github, _options);
        await Run();
        _others = _findings;

        // The repository that was refused had a stalled workflow earlier; its finding stays while it is inaccessible.
        _audit = new ActionsScheduleAuditCheck(_github, _options);
        _audit.Restore("{\"findings\":{\"actions-audit:stalled:Cratis/secret:old.yml\":\"was stalled\",\"actions-audit:stalled:Cratis/app:gone.yml\":\"resolved\"}}");
        _time.Advance(Hours(25));
        await Run();
        _keptWhileHidden = _findings;

        // Once it is accessible and healthy the retained finding is replaced by the audit.
        Workflows("Cratis/secret", Active(5, "ok.yml"));
        ScheduledRuns("Cratis/secret", (5, Hours(1)), (5, Hours(2)));
        _github.Respond("/repos/Cratis/forbidden/actions/workflows?per_page=100", new { workflows = Array.Empty<object>() });
        _time.Advance(Hours(25));
        await Run();
        _afterItBecameAccessible = _findings;

        // When every eligible repository refuses, nothing was audited.
        _audit = new ActionsScheduleAuditCheck(_github, _options);
        Repositories("Cratis/nope");
        _time.Advance(Hours(25));
        await Run();
        _allHidden = _findings;
        _stateAfterAllHidden = _audit.Persist()!;
    }

    [Fact] void should_audit_the_other_repositories_anyway() => _others.Select(finding => finding.Key).ShouldContainOnly("actions-audit:disabled:Cratis/app:x.yml");
    [Fact] void should_keep_the_finding_of_an_inaccessible_repository() => _keptWhileHidden.Select(finding => finding.Key).ShouldContain("actions-audit:stalled:Cratis/secret:old.yml");
    [Fact] void should_not_keep_the_finding_of_an_accessible_repository_that_resolved() => _keptWhileHidden.Select(finding => finding.Key).ShouldNotContain("actions-audit:stalled:Cratis/app:gone.yml");
    [Fact] void should_clear_a_retained_finding_once_the_repository_is_accessible_and_healthy() => _afterItBecameAccessible.Select(finding => finding.Key).ShouldNotContain("actions-audit:stalled:Cratis/secret:old.yml");
    [Fact] void should_be_unavailable_when_every_repository_refuses() => _allHidden.Select(finding => finding.Key).ShouldContain("actions-audit:unavailable");
    [Fact] void should_say_nothing_was_accessible() => _allHidden.Single(finding => finding.Key == "actions-audit:unavailable").Text.ShouldEqual("GitHub Actions audit unavailable: no repositories accessible");
    [Fact] void should_retry_in_an_hour() => _stateAfterAllHidden.ShouldContain("\"retry_at\":" + (_time.Now.ToUnixTimeSeconds() + 3600));
}
#endif
