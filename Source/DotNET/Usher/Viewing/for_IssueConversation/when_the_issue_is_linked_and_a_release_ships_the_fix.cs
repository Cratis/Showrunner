// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_the_issue_is_linked_and_a_release_ships_the_fix : a_read_model<IssueConversation>
{
    static readonly WorkItem _workItem = new("GitHub", "Cratis/Ledger#42", "Send button does nothing", "https://github.com/Cratis/Ledger/issues/42");
    IssueConversation _shipped;
    IssueConversation _issue;

    async Task Because()
    {
        var knownIssueId = new KnownIssueId(Guid.NewGuid());
        await Happened(
            IssueEvents.Described(Reporters.Subject),
            new SupportLinkedIssueToKnownIssue(knownIssueId, "Invoices are not sent"),
            new SupportLinkedIssueToWorkItem(_workItem),
            new ReleaseShippedFix("4.12.3", "Cratis/Ledger#42"));
        _shipped = _scenario.Instance!;
        await Happened(
            new SupportUnlinkedIssue(knownIssueId),
            new SupportUnlinkedWorkItem("Cratis/Ledger#42"),
            new ReporterReopenedIssue("Still broken in 4.12.3."));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_show_the_known_issue_when_linked() => _shipped.KnownIssueTitle.ShouldEqual(new KnownIssueTitle("Invoices are not sent"));
    [Fact] void should_show_the_work_item_when_linked() => _shipped.WorkItem.ShouldEqual(_workItem);
    [Fact] void should_show_the_version_with_the_fix() => _shipped.FixedInVersion.ShouldEqual(new ReleaseVersion("4.12.3"));
    [Fact] void should_have_no_known_issue_when_unlinked() => _issue.KnownIssueTitle.ShouldEqual(KnownIssueTitle.NotSet);
    [Fact] void should_have_no_work_item_when_unlinked() => _issue.WorkItem.ShouldEqual(Unset.WorkItem);
    [Fact] void should_clear_the_version_when_reopened() => _issue.FixedInVersion.ShouldEqual(ReleaseVersion.NotSet);

    [Fact]
    void should_record_the_links_and_the_release() => _issue.Entries.Skip(1).Select(_ => (_.Kind, _.AuthorKind, (string)_.Text)).ShouldEqual(
    [
        (ConversationEntryKind.LinkedToKnownIssue, ConversationAuthorKind.System, "Invoices are not sent"),
        (ConversationEntryKind.LinkedToWorkItem, ConversationAuthorKind.System, "Send button does nothing"),
        (ConversationEntryKind.FixReleased, ConversationAuthorKind.System, "4.12.3"),
        (ConversationEntryKind.UnlinkedFromKnownIssue, ConversationAuthorKind.System, string.Empty),
        (ConversationEntryKind.UnlinkedFromWorkItem, ConversationAuthorKind.System, "Cratis/Ledger#42"),
        (ConversationEntryKind.Reopened, ConversationAuthorKind.Reporter, "Still broken in 4.12.3.")
    ]);
}
#endif
