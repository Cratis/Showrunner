// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Issues.for_Issue;

public class when_the_desk_moves_the_issue_through_its_states : a_read_model<Issue>
{
    readonly List<IssueState> _states = [];

    async Task Because()
    {
        await Record(IssueEvents.Described(Reporters.Subject));
        await Record(new SupportTriagedIssue());
        await Record(new SupportAcceptedIssue());
        await Record(new SupportStartedWork());
        await Record(new SupportResolvedIssue("Fixed."));
        await Record(new ReporterReopenedIssue("Still broken."));
        await Record(new SupportDeclinedToFix("Works as designed."));
    }

    [Fact]
    void should_move_through_every_state() => _states.ShouldEqual(
    [
        IssueState.Reported,
        IssueState.Triaged,
        IssueState.Accepted,
        IssueState.InProgress,
        IssueState.Resolved,
        IssueState.Reported,
        IssueState.WontFix
    ]);

    async Task Record(object @event)
    {
        await Happened(@event);
        _states.Add(_scenario.Instance!.State);
    }
}
#endif
