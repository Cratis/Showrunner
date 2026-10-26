// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

public class when_the_issue_is_linked_and_unlinked : a_read_model<ReportedIssue>
{
    static readonly WorkItem _workItem = new("GitHub", "Cratis/Ledger#42", "Send button does nothing", "https://github.com/Cratis/Ledger/issues/42");
    ReportedIssue _linked;
    ReportedIssue _unlinked;

    async Task Because()
    {
        var knownIssueId = new KnownIssueId(Guid.NewGuid());
        await Happened(
            IssueEvents.Described(Reporters.Subject),
            new SupportLinkedIssueToKnownIssue(knownIssueId, "Invoices are not sent"),
            new SupportLinkedIssueToWorkItem(_workItem));
        _linked = _scenario.Instance!;
        await Happened(new SupportUnlinkedIssue(knownIssueId), new SupportUnlinkedWorkItem("Cratis/Ledger#42"));
        _unlinked = _scenario.Instance!;
    }

    [Fact] void should_show_the_known_issue_when_linked() => _linked.KnownIssueTitle.ShouldEqual(new KnownIssueTitle("Invoices are not sent"));
    [Fact] void should_show_the_work_item_when_linked() => _linked.WorkItem.ShouldEqual(_workItem);
    [Fact] void should_have_no_known_issue_when_unlinked() => _unlinked.KnownIssueTitle.ShouldEqual(KnownIssueTitle.NotSet);
    [Fact] void should_have_no_work_item_when_unlinked() => _unlinked.WorkItem.ShouldEqual(Unset.WorkItem);
}
#endif
