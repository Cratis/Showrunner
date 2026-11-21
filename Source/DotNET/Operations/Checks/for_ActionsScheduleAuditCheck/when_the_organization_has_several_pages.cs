// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_the_organization_has_several_pages : an_actions_audit
{
    void Establish()
    {
        _github.Respond(Organization, new[] { new { full_name = "Cratis/one" } }, "https://api.github.com/orgs/Cratis/repos?type=all&per_page=100&page=2");
        _github.Respond("/orgs/Cratis/repos?type=all&per_page=100&page=2", new[] { new { full_name = "Cratis/two" } });
        Workflows("Cratis/one", Active(1, "a.yml"));
        Workflows("Cratis/two", Active(2, "b.yml"));
        ScheduledRuns("Cratis/one", (1, Hours(1)), (1, Hours(2)));
        ScheduledRuns("Cratis/two", (2, Hours(1)), (2, Hours(2)));
    }

    async Task Because() => await Run();

    [Fact] void should_audit_the_repositories_of_every_page() => _github.Requests.ShouldContain("/repos/Cratis/two/actions/workflows?per_page=100");
    [Fact] void should_make_two_requests_per_repository_without_fanning_out_per_workflow() =>
        _github.Requests.Count(request => request.StartsWith("/repos/")).ShouldEqual(4);
    [Fact] void should_find_nothing_when_all_is_well() => _findings.ShouldBeEmpty();
    [Fact] void should_give_every_request_a_timeout_of_at_most_ten_seconds() => _github.Timeouts.All(timeout => timeout > TimeSpan.Zero && timeout <= TimeSpan.FromSeconds(10)).ShouldBeTrue();
}
#endif
