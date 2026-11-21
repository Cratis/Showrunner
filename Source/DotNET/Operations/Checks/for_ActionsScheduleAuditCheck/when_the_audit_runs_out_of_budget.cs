// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_the_audit_runs_out_of_budget : an_actions_audit
{
    IReadOnlyCollection<Alerts.Finding> _outOfRequests;
    string _cacheAfterwards;
    IReadOnlyCollection<Alerts.Finding> _outOfTime;
    int _requestsWithoutTime;

    async Task Because()
    {
        Repositories("Cratis/one", "Cratis/two");
        Workflows("Cratis/one", (1, "a.yml", "disabled_manually"));
        Workflows("Cratis/two", (2, "b.yml", "disabled_manually"));

        // A previous complete audit found something and knew a schedule.
        _options = new ActionsAuditOptions { MaximumRequests = 2 };
        _audit = new ActionsScheduleAuditCheck(_github, _options);
        _audit.Restore("{\"findings\":{\"actions-audit:stalled:Cratis/old:x.yml\":\"was stalled\"},\"scheduled\":[\"Cratis/old/7\"]}");
        await Run();
        _outOfRequests = _findings;
        _cacheAfterwards = _audit.Persist()!;

        // With no time left for collection no request is made at all, and the delivery reserve is untouched.
        _audit = null;
        _options = new ActionsAuditOptions();
        _github.Requests.Clear();
        await Run(budget: TimeSpan.Zero);
        _outOfTime = _findings;
        _requestsWithoutTime = _github.Requests.Count;
    }

    [Fact] void should_say_the_audit_is_incomplete() => _outOfRequests.Single(finding => finding.Key == "actions-audit:incomplete").Text.ShouldEqual("GitHub Actions audit incomplete: collection budget exhausted");
    [Fact] void should_keep_the_findings_of_the_part_it_saw() => _outOfRequests.Select(finding => finding.Key).ShouldContain("actions-audit:disabled:Cratis/one:a.yml");
    [Fact] void should_keep_what_the_previous_audit_found() => _outOfRequests.Select(finding => finding.Key).ShouldContain("actions-audit:stalled:Cratis/old:x.yml");
    [Fact] void should_not_replace_the_schedule_cache() => _cacheAfterwards.ShouldContain("Cratis/old/7");
    [Fact] void should_retry_the_next_day() => _cacheAfterwards.ShouldContain("\"retry_at\":" + (_time.Now.ToUnixTimeSeconds() + 86400));
    [Fact] void should_say_incomplete_when_there_is_no_time() => _outOfTime.Single().Key.ShouldEqual("actions-audit:incomplete");
    [Fact] void should_not_make_a_request_when_there_is_no_time() => _requestsWithoutTime.ShouldEqual(0);
}
#endif
