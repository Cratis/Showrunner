// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_pagination_points_somewhere_else : an_actions_audit
{
    IReadOnlyCollection<Alerts.Finding> _untrusted;
    IReadOnlyCollection<Alerts.Finding> _plainHttp;
    IReadOnlyCollection<Alerts.Finding> _emptyPage;
    int _emptyPageRequests;

    async Task Because()
    {
        _github.Respond(Organization, new[] { new { full_name = "Cratis/app" } }, "https://evil.example.com/orgs/Cratis/repos?page=2");
        Workflows("Cratis/app", (1, "a.yml", "disabled_manually"));
        await Run();
        _untrusted = _findings;

        _audit = null;
        _github.Respond(Organization, new[] { new { full_name = "Cratis/app" } }, "http://api.github.com/orgs/Cratis/repos?page=2");
        await Run();
        _plainHttp = _findings;

        // An empty page ends the walk even when it still advertises a next page.
        _audit = null;
        _github.Requests.Clear();
        _github.Respond(Organization, Array.Empty<object>(), "https://api.github.com/orgs/Cratis/repos?page=2");
        await Run();
        _emptyPage = _findings;
        _emptyPageRequests = _github.Requests.Count;
    }

    [Fact] void should_never_send_a_request_to_another_host() => _github.Uris.Where(uri => uri.Host != "api.github.com").ShouldBeEmpty();
    [Fact] void should_never_use_plain_http() => _github.Uris.Where(uri => uri.Scheme != "https").ShouldBeEmpty();
    [Fact] void should_report_the_audit_as_unavailable_for_an_untrusted_next_page() => _untrusted.Single(finding => finding.Key == "actions-audit:unavailable").Text.ShouldEqual("GitHub Actions audit unavailable: request or response failed");
    [Fact] void should_report_the_audit_as_unavailable_for_a_plain_http_next_page() => _plainHttp.Select(finding => finding.Key).ShouldContain("actions-audit:unavailable");
    [Fact] void should_stop_at_an_empty_page() => _emptyPage.ShouldBeEmpty();
    [Fact] void should_not_ask_for_the_page_after_an_empty_one() => _emptyPageRequests.ShouldEqual(1);
}
#endif
