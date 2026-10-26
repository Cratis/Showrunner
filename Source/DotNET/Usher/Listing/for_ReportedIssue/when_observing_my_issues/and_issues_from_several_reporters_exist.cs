// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue.when_observing_my_issues;

public class and_issues_from_several_reporters_exist : a_query<ReportedIssue>
{
    readonly IssueId _mine = IssueId.New();
    readonly IssueId _alsoMine = IssueId.New();
    readonly IssueId _someoneElses = IssueId.New();

    void Establish()
    {
        Given(_mine, IssueEvents.Described(Reporters.Subject));
        Given(_someoneElses, IssueEvents.Described(Reporters.SomeoneElse));
        Given(_alsoMine, IssueEvents.Flagged(Reporters.Subject));
    }

    async Task Because() => _result = await _scenario.Perform(nameof(ReportedIssue.MyIssues));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_only_the_reporters_issues() => ((IEnumerable<ReportedIssue>)_result.Data).Select(_ => _.Id).ShouldContainOnly(_mine, _alsoMine);
}
#endif
