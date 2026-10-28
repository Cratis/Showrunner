// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Queries;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation.when_observing_an_issue;

public class and_support_observes_the_issue_of_its_session : a_query<IssueConversation>
{
    readonly IssueId _issueId = IssueId.New();

    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);
    protected override VerifiedForward? Session => Reporters.LiveSessionOn(_issueId);

    void Establish() => Given(
        _issueId,
        IssueEvents.Described(Reporters.Identity),
        new ReporterSharedIdentity(Reporters.Reporter),
        new ReporterGrantedAccess(DateTimeOffset.UtcNow.AddHours(1)),
        new SupportSignedInAsReporter(Reporters.SessionId, IssueEvents.Support, IssueEvents.SupportPerson));

    async Task Because() => _result = await _scenario.Perform(nameof(IssueConversation.ForIssue), new QueryArguments { ["issueId"] = _issueId });

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_the_issue() => ((IEnumerable<IssueConversation>)_result.Data).Select(_ => _.Id).ShouldContainOnly(_issueId);
}
#endif
