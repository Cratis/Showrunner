// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue.when_observing_my_issues;

/// <summary>
/// The reporter granted access to one issue: support signed in as them lists only that issue.
/// </summary>
public class and_support_is_signed_in_as_the_reporter : a_query<ReportedIssue>
{
    readonly IssueId _sessionIssue = IssueId.New();

    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);
    protected override VerifiedForward? Session => Reporters.LiveSessionOn(_sessionIssue);

    void Establish()
    {
        Given(_sessionIssue, IssueEvents.Described(Reporters.Identity));
        Given(IssueId.New(), IssueEvents.Described(Reporters.Identity));
        Given(IssueId.New(), IssueEvents.Flagged(Reporters.Identity));
    }

    async Task Because() => _result = await _scenario.Perform(nameof(ReportedIssue.MyIssues));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_only_the_issue_of_the_session() => ((IEnumerable<ReportedIssue>)_result.Data).Select(_ => _.Id).ShouldContainOnly(_sessionIssue);
}
#endif
