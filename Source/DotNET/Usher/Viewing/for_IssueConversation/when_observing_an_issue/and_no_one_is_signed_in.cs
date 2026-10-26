// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Queries;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation.when_observing_an_issue;

public class and_no_one_is_signed_in : a_query<IssueConversation>
{
    readonly IssueId _issueId = IssueId.New();

    protected override ClaimsPrincipal Principal => Reporters.Anonymous();

    void Establish() => Given(_issueId, IssueEvents.Described(Reporters.Subject));

    async Task Because() => _result = await _scenario.Perform(nameof(IssueConversation.ForIssue), new QueryArguments { ["issueId"] = _issueId });

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();
}
#endif
