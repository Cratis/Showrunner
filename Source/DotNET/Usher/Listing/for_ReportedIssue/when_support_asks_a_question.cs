// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

public class when_support_asks_a_question : a_read_model<ReportedIssue>
{
    ReportedIssue _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Subject), new SupportAskedQuestion(QuestionId.New(), "Which invoice?", IssueEvents.Support));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_wait_on_the_reporter() => _issue.WaitingOnReporter.ShouldBeTrue();
    [Fact] void should_have_its_last_activity_when_the_question_was_asked() => _issue.LastActivityAt.ShouldEqual(At(1));
    [Fact] void should_keep_when_it_was_reported() => _issue.ReportedAt.ShouldEqual(At(0));
}
#endif
