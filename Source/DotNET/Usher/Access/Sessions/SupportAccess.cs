// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// What the issue's own events say about support's access to sign in as the reporter: who reported it, who they are
/// when they shared it, the grant in effect and the sessions support had. Usher decides every session from it.
/// </summary>
/// <remarks>
/// It is folded from the issue's events on every decision rather than read from a read model, because a session is a
/// security decision that must see a revocation the moment it is recorded, and because the decision's append is
/// guarded against exactly the events it was folded from. The grant follows the same rules as <see cref="Issue"/>:
/// a lapse of an earlier grant never ends a later one.
/// </remarks>
/// <param name="ReportedBy">Who reported the issue, at their identity provider; <see cref="UserIdentity.NotSet"/> when it was never reported.</param>
/// <param name="SharedIdentity">Who the reporter is, when they let support know; <see langword="null"/> until they do.</param>
/// <param name="Granted">Whether a grant is in effect, not revoked or lapsed; it may still have passed its end.</param>
/// <param name="ValidUntil">The end of the last grant; <see cref="Unset.ValidUntil"/> when there was none.</param>
/// <param name="Sessions">The sessions support had, by identifier.</param>
/// <param name="LastSequenceNumber">The sequence number of the last event it was folded from.</param>
public record SupportAccess(
    UserIdentity ReportedBy,
    Reporter? SharedIdentity,
    bool Granted,
    ValidUntil ValidUntil,
    ImmutableDictionary<SupportSessionId, SupportSessionFacts> Sessions,
    EventSequenceNumber LastSequenceNumber)
{
    /// <summary>
    /// The types of the events <see cref="SupportAccess"/> is folded from.
    /// </summary>
    public static readonly IReadOnlyList<Type> EventTypes =
    [
        typeof(ReporterDescribedIssue),
        typeof(ReporterFlaggedError),
        typeof(ReporterSharedIdentity),
        typeof(ReporterGrantedAccess),
        typeof(ReporterRevokedAccess),
        typeof(AccessGrantLapsed),
        typeof(SupportSignedInAsReporter),
        typeof(SupportSessionEnded)
    ];

    /// <summary>
    /// Nothing happened on the issue.
    /// </summary>
    public static readonly SupportAccess None = new(
        UserIdentity.NotSet,
        null,
        false,
        Unset.ValidUntil,
        [],
        EventSequenceNumber.Unavailable);

    /// <summary>
    /// Gets whether the issue was reported.
    /// </summary>
    public bool IsReported => ReportedBy.IsSet;

    /// <summary>
    /// Gets the sessions that started and have not ended.
    /// </summary>
    public IEnumerable<SupportSessionId> OpenSessions => Sessions.Where(_ => !_.Value.Ended).Select(_ => _.Key);

    /// <summary>
    /// Folds the issue's events.
    /// </summary>
    /// <param name="events">The issue's events, in order; events of other types are ignored.</param>
    /// <returns>The <see cref="SupportAccess"/>.</returns>
    public static SupportAccess From(IEnumerable<AppendedEvent> events) =>
        events.Aggregate(None, (access, @event) => access.Apply(@event.Content, @event.Context.SequenceNumber));

    /// <summary>
    /// Gets whether support can sign in as the reporter at a point in time.
    /// </summary>
    /// <param name="now">The point in time.</param>
    /// <returns>True when a grant is in effect and has not passed its end.</returns>
    public bool HasLiveGrant(DateTimeOffset now) => Granted && ValidUntil.Value > now;

    /// <summary>
    /// Gets whether a session started.
    /// </summary>
    /// <param name="sessionId">The session.</param>
    /// <returns>True when support signed in for it.</returns>
    public bool HasStarted(SupportSessionId sessionId) => Sessions.ContainsKey(sessionId);

    /// <summary>
    /// Gets whether a session ended.
    /// </summary>
    /// <param name="sessionId">The session.</param>
    /// <returns>True when it ended.</returns>
    public bool HasEnded(SupportSessionId sessionId) => Sessions.TryGetValue(sessionId, out var session) && session.Ended;

    /// <summary>
    /// Decides whether support may act as the reporter in a session at a point in time.
    /// </summary>
    /// <param name="request">The <see cref="SupportSessionRequest"/>.</param>
    /// <param name="now">The point in time.</param>
    /// <returns>Why support may not, or <see langword="null"/> when it may.</returns>
    public SupportSessionRefusal? RefusalFor(SupportSessionRequest request, DateTimeOffset now)
    {
        if (!IsReported)
        {
            return SupportSessionRefusal.IssueNotReported;
        }

        if (ReportedBy.Subject != request.Subject)
        {
            return SupportSessionRefusal.NotTheReporter;
        }

        // The same subject at another identity provider is somebody else; only the reporter's own identity is them.
        if (request.Actor == ReportedBy)
        {
            return SupportSessionRefusal.ActorIsTheReporter;
        }

        if (HasEnded(request.SessionId))
        {
            return SupportSessionRefusal.SessionEnded;
        }

        if (!HasLiveGrant(now))
        {
            return SupportSessionRefusal.NoLiveGrant;
        }

        return SharedIdentity is not null && UserIdentity.Of(SharedIdentity) == ReportedBy ? null : SupportSessionRefusal.IdentityNotShared;
    }

    SupportAccess Apply(object @event, EventSequenceNumber sequenceNumber)
    {
        var next = @event switch
        {
            ReporterDescribedIssue described when !IsReported => this with { ReportedBy = described.ReportedBy },
            ReporterFlaggedError flagged when !IsReported => this with { ReportedBy = flagged.ReportedBy },
            ReporterSharedIdentity shared => this with { SharedIdentity = shared.Reporter },
            ReporterGrantedAccess granted => this with { Granted = true, ValidUntil = granted.ValidUntil },
            ReporterRevokedAccess => this with { Granted = false },
            AccessGrantLapsed lapsed when lapsed.ValidUntil == ValidUntil => this with { Granted = false },
            SupportSignedInAsReporter signedIn when !HasStarted(signedIn.SessionId) =>
                this with { Sessions = Sessions.Add(signedIn.SessionId, new(signedIn.Author, sequenceNumber, false)) },
            SupportSessionEnded ended when HasStarted(ended.SessionId) =>
                this with { Sessions = Sessions.SetItem(ended.SessionId, Sessions[ended.SessionId] with { Ended = true }) },
            _ => this
        };

        return next with { LastSequenceNumber = sequenceNumber };
    }
}
