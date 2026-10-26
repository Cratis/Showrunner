// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Conversation.AnswerQuestion;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

public class when_the_reporter_answers_every_open_question : a_read_model<ReportedIssue>
{
    readonly QuestionId _first = QuestionId.New();
    readonly QuestionId _second = QuestionId.New();
    bool _waitingAfterFirstAnswer;
    ReportedIssue _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Subject),
            new SupportAskedQuestion(_first, "Which invoice?", IssueEvents.Support),
            new SupportAskedQuestion(_second, "Which browser?", IssueEvents.Agent),
            new ReporterAnsweredQuestion(_first, "March."));
        _waitingAfterFirstAnswer = _scenario.Instance!.WaitingOnReporter;
        await Happened(new ReporterAnsweredQuestion(_second, "Edge."));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_still_wait_while_a_question_is_open() => _waitingAfterFirstAnswer.ShouldBeTrue();
    [Fact] void should_not_wait_on_the_reporter_when_all_are_answered() => _issue.WaitingOnReporter.ShouldBeFalse();
    [Fact] void should_have_its_last_activity_when_the_last_answer_was_given() => _issue.LastActivityAt.ShouldEqual(At(4));
}
#endif
