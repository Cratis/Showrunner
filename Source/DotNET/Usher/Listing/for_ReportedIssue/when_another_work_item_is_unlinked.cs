// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

public class when_another_work_item_is_unlinked : a_read_model<ReportedIssue>
{
    static readonly WorkItem _workItem = new("GitHub", "Cratis/Ledger#42", "Send button does nothing", "https://github.com/Cratis/Ledger/issues/42");
    ReportedIssue _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Subject),
            new SupportLinkedIssueToWorkItem(_workItem),
            new SupportUnlinkedWorkItem("Cratis/Ledger#7"));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_keep_the_linked_work_item() => _issue.WorkItem.ShouldEqual(_workItem);
}
#endif
