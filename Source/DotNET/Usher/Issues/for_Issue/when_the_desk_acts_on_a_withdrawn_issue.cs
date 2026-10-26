// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Issues.for_Issue;

public class when_the_desk_acts_on_a_withdrawn_issue : a_read_model<Issue>
{
    Issue _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Subject), new ReporterWithdrewIssue(), new SupportAcceptedIssue(), new SupportResolvedIssue("Fixed."));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_stay_withdrawn() => _issue.State.ShouldEqual(IssueState.Withdrawn);
}
#endif
