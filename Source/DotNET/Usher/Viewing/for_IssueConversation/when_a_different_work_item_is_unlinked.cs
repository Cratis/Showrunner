// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_a_different_work_item_is_unlinked : a_read_model<IssueConversation>
{
    static readonly WorkItem _workItem = new("GitHub", "Cratis/Ledger#42", "Send button does nothing", "https://github.com/Cratis/Ledger/issues/42");
    IssueConversation _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Identity),
            new SupportLinkedIssueToWorkItem(_workItem),
            new SupportUnlinkedWorkItem("Cratis/Ledger#99"));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_keep_the_linked_work_item() => _issue.WorkItem.ShouldEqual(_workItem);
    [Fact] void should_still_record_the_unlinking() => _issue.Entries.Last().Kind.ShouldEqual(ConversationEntryKind.UnlinkedFromWorkItem);
}
#endif
