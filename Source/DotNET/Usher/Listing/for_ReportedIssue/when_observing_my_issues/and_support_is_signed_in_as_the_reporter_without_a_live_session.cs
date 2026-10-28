// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue.when_observing_my_issues;

/// <summary>
/// The grant behind the session lapsed: support signed in as the reporter lists nothing.
/// </summary>
public class and_support_is_signed_in_as_the_reporter_without_a_live_session : a_query<ReportedIssue>
{
    readonly IssueId _sessionIssue = IssueId.New();

    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);
    protected override VerifiedForward? Session => Reporters.SessionOn(_sessionIssue, SupportSessionRefusal.NoLiveGrant);

    void Establish() => Given(_sessionIssue, IssueEvents.Described(Reporters.Identity));

    async Task Because() => _result = await _scenario.Perform(nameof(ReportedIssue.MyIssues));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_nothing() => ((IEnumerable<ReportedIssue>)_result.Data).ShouldBeEmpty();
}
#endif
