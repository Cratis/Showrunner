// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Queries;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation.when_observing_an_issue;

/// <summary>
/// AuthProxy still vouches for the session, but the issue's events say it ended: support sees nothing.
/// </summary>
public class and_support_observes_the_issue_after_the_session_ended : a_query<IssueConversation>
{
    readonly IssueId _issueId = IssueId.New();

    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);
    protected override VerifiedForward? Session => Reporters.SessionOn(_issueId, SupportSessionRefusal.SessionEnded);

    void Establish() => Given(_issueId, IssueEvents.Described(Reporters.Identity));

    async Task Because() => _result = await _scenario.Perform(nameof(IssueConversation.ForIssue), new QueryArguments { ["issueId"] = _issueId });

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_nothing() => ((IEnumerable<IssueConversation>)_result.Data).ShouldBeEmpty();
}
#endif
