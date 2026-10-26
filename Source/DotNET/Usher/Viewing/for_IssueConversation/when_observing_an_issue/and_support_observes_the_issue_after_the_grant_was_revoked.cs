// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Queries;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation.when_observing_an_issue;

/// <summary>
/// The session was live when the request arrived, but the issue itself says the grant was revoked since: support sees
/// nothing, so an observation that outlives the revocation stops showing the issue.
/// </summary>
public class and_support_observes_the_issue_after_the_grant_was_revoked : a_query<IssueConversation>
{
    readonly IssueId _issueId = IssueId.New();

    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);
    protected override VerifiedForward? Session => Reporters.LiveSessionOn(_issueId);

    void Establish() => Given(
        _issueId,
        IssueEvents.Described(Reporters.Identity),
        new ReporterSharedIdentity(Reporters.Reporter),
        new ReporterGrantedAccess(DateTimeOffset.UtcNow.AddHours(1)),
        new SupportSignedInAsReporter(Reporters.SessionId, IssueEvents.Support),
        new ReporterRevokedAccess());

    async Task Because() => _result = await _scenario.Perform(nameof(IssueConversation.ForIssue), new QueryArguments { ["issueId"] = _issueId });

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_nothing() => ((IEnumerable<IssueConversation>)_result.Data).ShouldBeEmpty();
}
#endif
