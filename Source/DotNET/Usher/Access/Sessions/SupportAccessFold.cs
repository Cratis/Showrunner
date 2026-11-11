// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Folds the issue's events into <see cref="SupportAccess"/>. Events of other types change nothing but the sequence number.
/// </summary>
internal static class SupportAccessFold
{
    /// <summary>
    /// Applies one event.
    /// </summary>
    /// <param name="access">The access before the event.</param>
    /// <param name="event">The event.</param>
    /// <param name="sequenceNumber">The sequence number of the event.</param>
    /// <returns>The access after the event, which always has the sequence number of the event.</returns>
    internal static SupportAccess Apply(SupportAccess access, object @event, EventSequenceNumber sequenceNumber) =>
        Next(access, @event, sequenceNumber) with { LastSequenceNumber = sequenceNumber };

    static SupportAccess Next(SupportAccess access, object @event, EventSequenceNumber sequenceNumber) => @event switch
    {
        ReporterDescribedIssue or ReporterFlaggedError or ReporterSharedIdentity => Report(access, @event),
        ReporterGrantedAccess or ReporterWithdrewIssue or ReporterRevokedAccess or AccessGrantLapsed => Grant(access, @event),
        SupportSessionPermitted or SupportSignedInAsReporter or SupportSessionEnded => Session(access, @event, sequenceNumber),
        _ => access
    };

    static SupportAccess Report(SupportAccess access, object @event) => @event switch
    {
        ReporterDescribedIssue described when !access.IsReported => access with { ReportedBy = described.ReportedBy },
        ReporterFlaggedError flagged when !access.IsReported => access with { ReportedBy = flagged.ReportedBy },
        ReporterSharedIdentity shared => access with { SharedIdentity = shared.Reporter },
        _ => access
    };

    static SupportAccess Grant(SupportAccess access, object @event) => @event switch
    {
        ReporterGrantedAccess granted when !access.Withdrawn => access with { Granted = true, ValidUntil = granted.ValidUntil },
        ReporterWithdrewIssue => access with { Granted = false, Withdrawn = true },
        ReporterRevokedAccess => access with { Granted = false },
        AccessGrantLapsed lapsed when lapsed.ValidUntil == access.ValidUntil => access with { Granted = false },
        _ => access
    };

    static SupportAccess Session(SupportAccess access, object @event, EventSequenceNumber sequenceNumber) => @event switch
    {
        SupportSessionPermitted permitted => access with { Permitted = access.Permitted.Add(permitted.SessionId) },
        SupportSignedInAsReporter signedIn when !access.HasStarted(signedIn.SessionId) =>
            access with { Sessions = access.Sessions.Add(signedIn.SessionId, new(signedIn.Author, sequenceNumber, false)) },
        SupportSessionEnded ended when access.HasStarted(ended.SessionId) =>
            access with { Sessions = access.Sessions.SetItem(ended.SessionId, access.Sessions[ended.SessionId] with { Ended = true }) },
        _ => access
    };
}
