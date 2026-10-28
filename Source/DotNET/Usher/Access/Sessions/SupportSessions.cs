// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventSequences.Concurrency;
using Cratis.Monads;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Decides and records the sessions in which support signs in as a reporter, from the issue's own events.
/// </summary>
/// <remarks>
/// <para>
/// Every decision folds <see cref="SupportAccess"/> from the issue's events at that moment; nothing is cached, so a
/// revocation counts from the instant it is recorded. A start is appended guarded by a concurrency scope on exactly
/// those events, so a grant revoked or lapsing between the decision and the append refuses the start instead of
/// recording a session on a grant that is gone.
/// </para>
/// <para>
/// Starts and ends are appended once per session, with an idempotency key from the issue and the session: AuthProxy
/// replicas, the reporter's revocation and the lapse of the grant can all report the same end, and only the first is
/// recorded.
/// </para>
/// </remarks>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class SupportSessions(TimeProvider time, ILogger<SupportSessions> logger)
{
    const int MaximumAttempts = 3;

    /// <summary>
    /// Reads what the issue's events say about support's access.
    /// </summary>
    /// <param name="eventStore">The event store of the issue's namespace.</param>
    /// <param name="issueId">The issue.</param>
    /// <returns>The <see cref="SupportAccess"/>.</returns>
    public async Task<SupportAccess> Read(IEventStore eventStore, IssueId issueId) =>
        SupportAccess.From(await ReadEvents(eventStore, issueId));

    /// <summary>
    /// Answers AuthProxy's grant check: whether support may act as the reporter in a session right now.
    /// </summary>
    /// <remarks>
    /// AuthProxy checks the grant once before it announces a new session, with the identifier the session will have,
    /// and again while the session lasts. A session that has not started is therefore live once: the first check
    /// records <see cref="SupportSessionPermitted"/>, and every later check for it is not live until Usher has
    /// acknowledged its start. A session Usher never recorded as started can not be kept alive by asking again, on this
    /// instance or any other.
    /// </remarks>
    /// <param name="eventStore">The event store of the issue's namespace.</param>
    /// <param name="request">The <see cref="SupportSessionRequest"/>.</param>
    /// <returns>The <see cref="LiveGrant"/>, or why there is none.</returns>
    /// <exception cref="SupportSessionNotRecorded">The event log refused to record that the session was permitted.</exception>
    public async Task<Result<LiveGrant, SupportSessionRefusal>> Check(IEventStore eventStore, SupportSessionRequest request)
    {
        var access = await Read(eventStore, request.IssueId);
        if (access.RefusalFor(request, time.GetUtcNow()) is { } refusal)
        {
            logger.SessionRefused(request.IssueId, refusal, eventStore.Namespace);
            return refusal;
        }

        if (!access.HasStarted(request.SessionId) && !await Permit(eventStore, request, access))
        {
            logger.SessionRefused(request.IssueId, SupportSessionRefusal.SessionNotStarted, eventStore.Namespace);
            return SupportSessionRefusal.SessionNotStarted;
        }

        return new LiveGrant(access.ValidUntil, access.SharedIdentity!);
    }

    /// <summary>
    /// Checks whether support may act as the reporter right now in a session that started: the session started and has
    /// not ended, and the grant lets support act.
    /// </summary>
    /// <param name="eventStore">The event store of the issue's namespace.</param>
    /// <param name="request">The <see cref="SupportSessionRequest"/>.</param>
    /// <returns>The <see cref="LiveGrant"/>, or why there is none.</returns>
    public async Task<Result<LiveGrant, SupportSessionRefusal>> CheckSession(IEventStore eventStore, SupportSessionRequest request)
    {
        var access = await Read(eventStore, request.IssueId);
        var refusal = access.RefusalFor(request, time.GetUtcNow()) ??
            (access.HasStarted(request.SessionId) ? null : SupportSessionRefusal.SessionNotStarted);
        if (refusal is { } refused)
        {
            logger.SessionRefused(request.IssueId, refused, eventStore.Namespace);
            return refused;
        }

        return new LiveGrant(access.ValidUntil, access.SharedIdentity!);
    }

    /// <summary>
    /// Records that support signed in as the reporter, when the grant lets it.
    /// </summary>
    /// <param name="eventStore">The event store of the issue's namespace.</param>
    /// <param name="request">The <see cref="SupportSessionRequest"/>.</param>
    /// <param name="author">Who signed in, as the reporter is shown them.</param>
    /// <returns>How the start was recorded, or why it was refused.</returns>
    /// <exception cref="SupportSessionNotRecorded">The event log refused the start for another reason than a change on the issue.</exception>
    public async Task<Result<SupportSessionStart, SupportSessionRefusal>> Start(IEventStore eventStore, SupportSessionRequest request, Contracts.Conversation.SupportAuthor author)
    {
        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            var eventTypes = EventTypesIn(eventStore);
            var access = SupportAccess.From(await eventStore.EventLog.GetForEventSourceIdAndEventTypes(request.IssueId, eventTypes));
            if (access.RefusalFor(request, time.GetUtcNow()) is { } refusal)
            {
                logger.SessionRefused(request.IssueId, refusal, eventStore.Namespace);
                return refusal;
            }

            if (access.HasStarted(request.SessionId))
            {
                return SupportSessionStart.AlreadyStarted;
            }

            var result = await eventStore.EventLog.AppendOnce(
                request.IssueId,
                new SupportSignedInAsReporter(request.SessionId, author, request.Actor),
                KeyFor("started", request.IssueId, request.SessionId),
                concurrencyScope: new ConcurrencyScope(access.LastSequenceNumber, request.IssueId, EventTypes: eventTypes));

            if (result.HasConcurrencyViolations)
            {
                // The issue's access changed between the read and the append; decide again on what it is now.
                logger.AccessChangedWhileStarting(request.IssueId, eventStore.Namespace);
                continue;
            }

            if (result.WasAlreadyAppended || result.HasIdempotencyConflicts)
            {
                return SupportSessionStart.AlreadyStarted;
            }

            if (!result.IsSuccess)
            {
                throw new SupportSessionNotRecorded(typeof(SupportSignedInAsReporter), request.IssueId, eventStore.Namespace, result);
            }

            logger.SessionStarted(request.IssueId, eventStore.Namespace);
            return SupportSessionStart.Started;
        }

        logger.SessionRefused(request.IssueId, SupportSessionRefusal.Contended, eventStore.Namespace);
        return SupportSessionRefusal.Contended;
    }

    /// <summary>
    /// Records that a session ended, once, however many times the end is reported.
    /// </summary>
    /// <param name="eventStore">The event store of the issue's namespace.</param>
    /// <param name="issueId">The issue.</param>
    /// <param name="sessionId">The session.</param>
    /// <returns>How the end was recorded.</returns>
    /// <exception cref="SupportSessionNotRecorded">The event log refused the end.</exception>
    public async Task<SupportSessionEnd> End(IEventStore eventStore, IssueId issueId, SupportSessionId sessionId)
    {
        var access = await Read(eventStore, issueId);
        if (!access.HasStarted(sessionId))
        {
            logger.EndOfUnknownSession(issueId, eventStore.Namespace);
            return SupportSessionEnd.NeverStarted;
        }

        if (access.HasEnded(sessionId))
        {
            return SupportSessionEnd.AlreadyEnded;
        }

        var result = await eventStore.EventLog.AppendOnce(issueId, new SupportSessionEnded(sessionId), KeyFor("ended", issueId, sessionId));
        if (result.WasAlreadyAppended || result.HasIdempotencyConflicts)
        {
            return SupportSessionEnd.AlreadyEnded;
        }

        if (!result.IsSuccess)
        {
            throw new SupportSessionNotRecorded(typeof(SupportSessionEnded), issueId, eventStore.Namespace, result);
        }

        logger.SessionEnded(issueId, eventStore.Namespace);
        return SupportSessionEnd.Ended;
    }

    /// <summary>
    /// Ends every session that was open when the grant ended, when the event at a sequence number ended it: the
    /// reporter revoked it, or the grant in effect lapsed.
    /// </summary>
    /// <param name="eventStore">The event store of the issue's namespace.</param>
    /// <param name="issueId">The issue.</param>
    /// <param name="sequenceNumber">The sequence number of the event that may have ended the grant.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="SupportSessionNotRecorded">The event log refused an end.</exception>
    public async Task EndSessionsOfEndedGrant(IEventStore eventStore, IssueId issueId, EventSequenceNumber sequenceNumber)
    {
        var events = await ReadEvents(eventStore, issueId);
        var before = SupportAccess.From(events.Where(_ => _.Context.SequenceNumber < sequenceNumber));
        var after = SupportAccess.From(events.Where(_ => _.Context.SequenceNumber <= sequenceNumber));
        if (!before.Granted || after.Granted)
        {
            return;
        }

        var now = SupportAccess.From(events);
        foreach (var sessionId in before.OpenSessions.Where(_ => !now.HasEnded(_)))
        {
            await End(eventStore, issueId, sessionId);
        }
    }

    static async Task<bool> Permit(IEventStore eventStore, SupportSessionRequest request, SupportAccess access)
    {
        if (access.WasPermitted(request.SessionId))
        {
            return false;
        }

        // Once per session, whichever instance answers: a second check for the same session that has not started finds
        // the first one's permit and is not live.
        var result = await eventStore.EventLog.AppendOnce(
            request.IssueId,
            new SupportSessionPermitted(request.SessionId),
            KeyFor("permitted", request.IssueId, request.SessionId));
        if (result.WasAlreadyAppended || result.HasIdempotencyConflicts)
        {
            return false;
        }

        if (!result.IsSuccess)
        {
            throw new SupportSessionNotRecorded(typeof(SupportSessionPermitted), request.IssueId, eventStore.Namespace, result);
        }

        return true;
    }

    static IdempotencyKey KeyFor(string what, IssueId issueId, SupportSessionId sessionId) =>
        new($"usher-session-{what}-{issueId.Value:D}-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId.Value)))}");

    static EventType[] EventTypesIn(IEventStore eventStore) => [.. SupportAccess.EventTypes.Select(eventStore.EventTypes.GetEventTypeFor)];

    static Task<System.Collections.Immutable.IImmutableList<AppendedEvent>> ReadEvents(IEventStore eventStore, IssueId issueId) =>
        eventStore.EventLog.GetForEventSourceIdAndEventTypes(issueId, EventTypesIn(eventStore));
}
