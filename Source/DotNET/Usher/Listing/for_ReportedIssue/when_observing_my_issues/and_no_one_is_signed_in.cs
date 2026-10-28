// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue.when_observing_my_issues;

public class and_no_one_is_signed_in : a_query<ReportedIssue>
{
    protected override ClaimsPrincipal Principal => Reporters.Anonymous();

    void Establish() => Given(IssueId.New(), IssueEvents.Described(Reporters.Identity));

    async Task Because() => _result = await _scenario.Perform(nameof(ReportedIssue.MyIssues));

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();
}
#endif
