// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

/// <summary>
/// Every fact on the issue counts as activity, including those that do not change what the list shows.
/// </summary>
public class when_anything_happens_on_the_issue : a_read_model<ReportedIssue>
{
    readonly List<DateTimeOffset> _lastActivities = [];

    async Task Because()
    {
        var sessionId = SupportSessionId.New();
        object[] events =
        [
            IssueEvents.Described(Reporters.Subject),
            new ReporterSharedIdentity(Reporters.Reporter),
            new ReporterAddedDetails("Every Monday."),
            new SupportRequestedAccess("To see the invoice."),
            new ReporterGrantedAccess(Start.AddHours(1)),
            new SupportSignedInAsReporter(sessionId, IssueEvents.Support),
            new SupportSessionEnded(sessionId),
            new ReporterRevokedAccess(),
            new ReporterGrantedAccess(Start.AddHours(2)),
            new AccessGrantLapsed(Start.AddHours(2)),
            new SupportReplied("Thanks, looking into it.", IssueEvents.Support)
        ];

        foreach (var @event in events)
        {
            await Happened(@event);
            _lastActivities.Add(_scenario.Instance!.LastActivityAt);
        }
    }

    [Fact] void should_move_the_last_activity_with_every_event() => _lastActivities.ShouldEqual(Enumerable.Range(0, 11).Select(_ => At((ulong)_)));
}
#endif
