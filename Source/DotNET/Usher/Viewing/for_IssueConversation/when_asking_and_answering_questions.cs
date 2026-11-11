// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Conversation.AnswerQuestion;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_asking_and_answering_questions : a_read_model<IssueConversation>
{
    readonly QuestionId _first = QuestionId.New();
    readonly QuestionId _second = QuestionId.New();
    IssueConversation _asked;
    IssueConversation _duplicate;
    IssueConversation _answered;
    IssueConversation _finished;

    async Task Because()
    {
        await Happened(IssueEvents.Flagged(Reporters.Identity), new SupportAskedQuestion(_first, "Which invoice?", IssueEvents.Support), new SupportAskedQuestion(_second, "Which browser?", IssueEvents.Agent));
        _asked = _scenario.Instance!;
        await Happened(new SupportAskedQuestion(_second, "Duplicate", IssueEvents.Support));
        _duplicate = _scenario.Instance!;
        await Happened(new ReporterAnsweredQuestion(_second, "Firefox"));
        _answered = _scenario.Instance!;
        await Happened(new ReporterAnsweredQuestion(_first, "Invoice 123"));
        _finished = _scenario.Instance!;
    }

    [Fact] void should_open_with_the_error_message() => _asked.Entries.First().Kind.ShouldEqual(ConversationEntryKind.ErrorReported);
    [Fact] void should_show_the_latest_unanswered_question() => _asked.OpenQuestion.QuestionId.ShouldEqual(_second);
    [Fact] void should_ignore_duplicate_questions() => _duplicate.Entries.ShouldContainOnly(_asked.Entries.ToArray());
    [Fact] void should_return_to_the_previous_question() => _answered.OpenQuestion.QuestionId.ShouldEqual(_first);
    [Fact] void should_clear_the_last_answered_question() => _finished.OpenQuestion.ShouldEqual(OpenQuestion.None);
    [Fact] void should_record_the_answer() => _finished.Entries.Last().Text.ShouldEqual((ConversationEntryText)"Invoice 123");
}
#endif
