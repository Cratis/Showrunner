// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Conversation.AnswerQuestion.when_answering_a_question;

public class and_the_question_is_open : an_issue_command<AnswerQuestion>
{
    readonly QuestionId _questionId = QuestionId.New();

    void Establish() => GivenReported(new SupportAskedQuestion(_questionId, "Which invoice?", IssueEvents.Support));

    async Task Because() => _result = await _scenario.Execute(new AnswerQuestion(_issueId, _questionId, "The one for March."));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_answer_to_the_question() =>
        await _scenario.ShouldHaveAppendedEvent<AnswerQuestion, ReporterAnsweredQuestion>(_issueId, e => e.QuestionId == _questionId && e.Answer == "The one for March.");
}
#endif
