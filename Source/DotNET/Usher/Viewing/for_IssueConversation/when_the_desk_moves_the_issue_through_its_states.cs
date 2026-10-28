// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_the_desk_moves_the_issue_through_its_states : a_read_model<IssueConversation>
{
    readonly List<IssueState> _states = [];
    IssueConversation _issue;

    async Task Because()
    {
        await Record(IssueEvents.Described(Reporters.Identity));
        await Record(new SupportTriagedIssue());
        await Record(new SupportAcceptedIssue());
        await Record(new SupportStartedWork());
        await Record(new SupportResolvedIssue("Fixed the send button."));
        await Record(new ReporterReopenedIssue("Still broken."));
        await Record(new SupportDeclinedToFix("Works as designed."));
        await Record(new ReporterWithdrewIssue());
        await Record(new SupportAcceptedIssue());
        _issue = _scenario.Instance!;
    }

    [Fact]
    void should_move_through_every_state_and_stay_withdrawn() => _states.ShouldEqual(
    [
        IssueState.Reported,
        IssueState.Triaged,
        IssueState.Accepted,
        IssueState.InProgress,
        IssueState.Resolved,
        IssueState.Reported,
        IssueState.WontFix,
        IssueState.Withdrawn,
        IssueState.Withdrawn
    ]);

    [Fact]
    void should_record_each_change_with_who_made_it_and_why() => _issue.Entries.Skip(1).Select(_ => (_.Kind, _.AuthorKind, (string)_.Text)).ShouldEqual(
    [
        (ConversationEntryKind.Triaged, ConversationAuthorKind.System, string.Empty),
        (ConversationEntryKind.Accepted, ConversationAuthorKind.System, string.Empty),
        (ConversationEntryKind.WorkStarted, ConversationAuthorKind.System, string.Empty),
        (ConversationEntryKind.Resolved, ConversationAuthorKind.System, "Fixed the send button."),
        (ConversationEntryKind.Reopened, ConversationAuthorKind.Reporter, "Still broken."),
        (ConversationEntryKind.DeclinedToFix, ConversationAuthorKind.System, "Works as designed."),
        (ConversationEntryKind.Withdrawn, ConversationAuthorKind.Reporter, string.Empty),
        (ConversationEntryKind.Accepted, ConversationAuthorKind.System, string.Empty)
    ]);

    async Task Record(object @event)
    {
        await Happened(@event);
        _states.Add(_scenario.Instance!.State);
    }
}
#endif
