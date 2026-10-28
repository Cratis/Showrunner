// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Conversation.AnswerQuestion.when_answering_a_question;

public class and_the_question_is_already_answered : an_issue_command<AnswerQuestion>
{
    readonly QuestionId _questionId = QuestionId.New();

    void Establish() => GivenReported(
        new SupportAskedQuestion(_questionId, "Which invoice?", IssueEvents.Support),
        new ReporterAnsweredQuestion(_questionId, "The one for March."));

    async Task Because() => _result = await _scenario.Execute(new AnswerQuestion(_issueId, _questionId, "Actually, April."));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
