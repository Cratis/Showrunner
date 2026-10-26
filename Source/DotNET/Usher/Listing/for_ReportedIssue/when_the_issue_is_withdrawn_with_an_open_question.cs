// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

public class when_the_issue_is_withdrawn_with_an_open_question : a_read_model<ReportedIssue>
{
    ReportedIssue _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Subject),
            new SupportAskedQuestion(QuestionId.New(), "Which invoice?", IssueEvents.Support),
            new ReporterWithdrewIssue());
        _issue = _scenario.Instance!;
    }

    [Fact] void should_not_wait_on_the_reporter() => _issue.WaitingOnReporter.ShouldBeFalse();
}
#endif
