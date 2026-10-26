// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Queries;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation.when_observing_an_issue;

public class and_someone_else_reported_it : a_query<IssueConversation>
{
    readonly IssueId _issueId = IssueId.New();

    void Establish() => Given(_issueId, IssueEvents.Described(Reporters.SomeoneElse));

    async Task Because() => _result = await _scenario.Perform(nameof(IssueConversation.ForIssue), new QueryArguments { ["issueId"] = _issueId });

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_nothing() => _result.Data.ShouldBeNull();
}
#endif
