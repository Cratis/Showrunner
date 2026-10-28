// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Conversation.AnswerQuestion;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_support_and_the_reporter_converse : a_read_model<IssueConversation>
{
    static readonly SupportAuthor _hiddenPerson = new(SupportAuthorName.NotSet, SupportAuthorKind.Person);
    readonly QuestionId _first = QuestionId.New();
    readonly QuestionId _second = QuestionId.New();
    OpenQuestion _openAfterBothAsked;
    IssueConversation _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Identity),
            new SupportAskedQuestion(_first, "Which invoice?", IssueEvents.Support),
            new SupportAskedQuestion(_second, "Which browser?", IssueEvents.Agent),
            new SupportAskedQuestion(_second, "Which browser?", IssueEvents.Agent));
        _openAfterBothAsked = _scenario.Instance!.OpenQuestion;
        await Happened(
            new ReporterAnsweredQuestion(_second, "Edge."),
            new SupportReplied("Thanks, looking into it.", _hiddenPerson));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_have_the_latest_question_open_while_both_are_unanswered() => _openAfterBothAsked.ShouldEqual(new OpenQuestion(_second, "Which browser?"));
    [Fact] void should_have_the_earlier_question_open_once_the_latest_is_answered() => _issue.OpenQuestion.ShouldEqual(new OpenQuestion(_first, "Which invoice?"));

    [Fact]
    void should_record_every_entry_once_in_order() => _issue.Entries.Select(_ => (_.Kind, _.AuthorKind, _.AuthorName, _.QuestionId)).ShouldEqual(
    [
        (ConversationEntryKind.IssueDescribed, ConversationAuthorKind.Reporter, SupportAuthorName.NotSet, new QuestionId(Guid.Empty)),
        (ConversationEntryKind.QuestionAsked, ConversationAuthorKind.Support, new SupportAuthorName("Grace"), _first),
        (ConversationEntryKind.QuestionAsked, ConversationAuthorKind.Agent, new SupportAuthorName("Usher Agent"), _second),
        (ConversationEntryKind.QuestionAnswered, ConversationAuthorKind.Reporter, SupportAuthorName.NotSet, _second),
        (ConversationEntryKind.Replied, ConversationAuthorKind.Support, SupportAuthorName.NotSet, new QuestionId(Guid.Empty))
    ]);

    [Fact] void should_have_the_texts() => _issue.Entries.Skip(1).Select(_ => (string)_.Text).ShouldEqual(["Which invoice?", "Which browser?", "Edge.", "Thanks, looking into it."]);
}
#endif
