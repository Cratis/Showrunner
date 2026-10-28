// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_an_issue_is_described : a_read_model<IssueConversation>
{
    IssueConversation _issue;
    ConversationEntry _entry;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Identity));
        _issue = _scenario.Instance!;
        _entry = _issue.Entries.Single();
    }

    [Fact] void should_be_the_issue() => _issue.Id.ShouldEqual(_issueId);
    [Fact] void should_have_the_summary_as_title() => _issue.Title.ShouldEqual(new IssueTitle("The invoice is not sent"));
    [Fact] void should_be_reported() => _issue.State.ShouldEqual(IssueState.Reported);
    [Fact] void should_have_the_location() => _issue.Location.ShouldEqual(IssueEvents.Location);
    [Fact] void should_be_reported_when_it_happened() => _issue.ReportedAt.ShouldEqual(At(0));
    [Fact] void should_not_have_identity_shared() => _issue.IdentityShared.ShouldBeFalse();
    [Fact] void should_have_no_open_question() => _issue.OpenQuestion.ShouldEqual(OpenQuestion.None);
    [Fact] void should_have_no_access() => _issue.Access.ShouldEqual(IssueAccess.None);
    [Fact] void should_not_be_linked_to_a_known_issue() => _issue.KnownIssueTitle.ShouldEqual(KnownIssueTitle.NotSet);
    [Fact] void should_not_be_linked_to_a_work_item() => _issue.WorkItem.ShouldEqual(Unset.WorkItem);
    [Fact] void should_not_be_fixed_in_a_release() => _issue.FixedInVersion.ShouldEqual(ReleaseVersion.NotSet);
    [Fact] void should_belong_to_the_reporter() => _issue.ReportedBy.ShouldEqual(Reporters.Identity);
    [Fact] void should_start_with_the_description() => _entry.Kind.ShouldEqual(ConversationEntryKind.IssueDescribed);
    [Fact] void should_have_the_description_as_text() => _entry.Text.ShouldEqual(new ConversationEntryText("I pressed send and nothing happened."));
    [Fact] void should_be_from_the_reporter() => _entry.AuthorKind.ShouldEqual(ConversationAuthorKind.Reporter);
    [Fact] void should_have_no_author_name() => _entry.AuthorName.ShouldEqual(SupportAuthorName.NotSet);
    [Fact] void should_be_at_when_it_happened() => _entry.At.ShouldEqual(At(0));
    [Fact] void should_be_identified_by_its_sequence_number() => _entry.EntryId.ShouldEqual(new ConversationEntryId(0));
}
#endif
