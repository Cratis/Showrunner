// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Queries;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation.when_observing_an_issue;

public class and_the_reporter_reported_it : a_query<IssueConversation>
{
    readonly IssueId _issueId = IssueId.New();

    void Establish()
    {
        Given(_issueId, IssueEvents.Described(Reporters.Identity));
        Given(IssueId.New(), IssueEvents.Described(Reporters.Identity));
    }

    async Task Because() => _result = await _scenario.Perform(nameof(IssueConversation.ForIssue), new QueryArguments { ["issueId"] = _issueId });

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_only_the_issue() => ((IEnumerable<IssueConversation>)_result.Data).Select(_ => _.Id).ShouldContainOnly(_issueId);
}
#endif
