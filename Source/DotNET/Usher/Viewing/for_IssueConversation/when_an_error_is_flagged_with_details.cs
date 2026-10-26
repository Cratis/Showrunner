// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_an_error_is_flagged_with_details : a_read_model<IssueConversation>
{
    IssueConversation _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Flagged(Reporters.Identity), new ReporterAddedDetails("I was sending the March invoice."));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_have_the_error_message_as_title() => _issue.Title.ShouldEqual(new IssueTitle("Object reference not set to an instance of an object."));

    [Fact]
    void should_have_the_error_then_the_details() => _issue.Entries.Select(_ => (_.Kind, _.Text, _.AuthorKind)).ShouldEqual(
    [
        (ConversationEntryKind.ErrorReported, new ConversationEntryText("Object reference not set to an instance of an object."), ConversationAuthorKind.Reporter),
        (ConversationEntryKind.DetailsAdded, new ConversationEntryText("I was sending the March invoice."), ConversationAuthorKind.Reporter)
    ]);
}
#endif
