// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Conversation.AnswerQuestion;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Issues.for_Issue;

public class when_questions_are_asked_and_answered : a_read_model<Issue>
{
    readonly QuestionId _answered = QuestionId.New();
    readonly QuestionId _open = QuestionId.New();
    Issue _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Identity),
            new ReporterSharedIdentity(Reporters.Reporter),
            new SupportAskedQuestion(_answered, "Which invoice?", IssueEvents.Support),
            new SupportAskedQuestion(_open, "Which browser?", IssueEvents.Agent),
            new SupportAskedQuestion(_open, "Which browser?", IssueEvents.Agent),
            new ReporterAnsweredQuestion(_answered, "March."));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_have_only_the_unanswered_question_open_once() => _issue.OpenQuestions.ShouldContainOnly(_open);
    [Fact] void should_have_identity_shared() => _issue.IdentityShared.ShouldBeTrue();
}
#endif
