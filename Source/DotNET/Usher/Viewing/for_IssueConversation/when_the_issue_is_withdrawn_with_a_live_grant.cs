// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_the_issue_is_withdrawn_with_a_live_grant : a_read_model<IssueConversation>
{
    IssueConversation _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Identity), new ReporterGrantedAccess(Start.AddHours(1)), new ReporterWithdrewIssue());
        _issue = _scenario.Instance!;
    }

    [Fact] void should_be_withdrawn() => _issue.State.ShouldEqual(IssueState.Withdrawn);
    [Fact] void should_end_the_grant() => _issue.Access.State.ShouldEqual(AccessState.EndedByWithdrawal);
}
#endif
