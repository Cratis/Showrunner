// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventSequences.Concurrency;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Access.Lapsing;

/// <summary>
/// A grant of access that has not ended yet, one per issue. The lapsing automation reads it to find the grants whose
/// end has passed; a grant leaves it when the reporter revokes it or it lapses.
/// </summary>
/// <param name="Id">The issue the grant is on.</param>
/// <param name="ValidUntil">When the grant ends.</param>
public record GrantToLapse(IssueId Id, ValidUntil ValidUntil)
{
    /// <summary>
    /// Records a grant; a new grant replaces the one before it.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static GrantToLapse Reduce(ReporterGrantedAccess @event, GrantToLapse? current, EventContext context) =>
        new(new IssueId(Guid.Parse(context.EventSourceId.Value)), @event.ValidUntil);

    /// <summary>
    /// Ends the grant when the reporter revokes it.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static GrantToLapse? Reduce(ReporterRevokedAccess @event, GrantToLapse? current) => null;

    /// <summary>
    /// Ends the grant when the reporter withdraws the issue; nothing is left to lapse.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static GrantToLapse? Reduce(ReporterWithdrewIssue @event, GrantToLapse? current) => null;

    /// <summary>
    /// Ends the grant when the grant that lapsed is the current one; the lapse of an earlier grant leaves a later one.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static GrantToLapse? Reduce(AccessGrantLapsed @event, GrantToLapse? current) =>
        current is null || current.ValidUntil == @event.ValidUntil ? null : current;

    /// <summary>
    /// Gets whether the grant has passed its end at a point in time.
    /// </summary>
    /// <param name="now">The point in time.</param>
    /// <returns>True when the end is at or before <paramref name="now"/>.</returns>
    public bool IsDue(DateTimeOffset now) => ValidUntil.Value <= now;
}

/// <summary>
/// Appends <see cref="AccessGrantLapsed"/> for every grant in a namespace whose end has passed without the reporter
/// revoking it. It runs on Usher's background schedule, once per namespace, so a grant lapses within one
/// <see cref="UsherOptions.PollingInterval"/> of its end - also when the application was not running at that moment.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GrantToLapse"/> only nominates candidates; the decision is taken against the issue's own events. The
/// lapse is appended only when the last access event on the issue is a grant whose end has passed, and the append is
/// guarded by a concurrency scope on those events, so a grant revoked or replaced in the meantime is never lapsed. It
/// carries the grant's exact end, and it is appended once per grant: a second instance lapsing the same grant at the
/// same time appends nothing.
/// </para>
/// <para>
/// Support can never sign in on a grant past its end even before the lapse is recorded:
/// <c>Issue.HasLiveGrant</c> compares the end with the clock. The lapse is the recorded fact, published as
/// <c>AccessExpired</c>.
/// </para>
/// </remarks>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class GrantLapsing(TimeProvider time, ILogger<GrantLapsing> logger)
{
    static readonly Type[] _accessEvents = [typeof(ReporterGrantedAccess), typeof(ReporterRevokedAccess), typeof(AccessGrantLapsed)];

    /// <summary>
    /// Lapses every grant in a namespace whose end has passed.
    /// </summary>
    /// <param name="eventStore">The event store of the namespace.</param>
    /// <returns>An awaitable task.</returns>
    public async Task LapseDueGrants(IEventStore eventStore)
    {
        var now = time.GetUtcNow();
        var grants = await eventStore.ReadModels.GetInstances<GrantToLapse>();
        foreach (var grant in grants.Where(_ => _.IsDue(now)))
        {
            await Lapse(eventStore, grant.Id);
        }
    }

    /// <summary>
    /// Lapses the grant in effect on an issue, when its end has passed.
    /// </summary>
    /// <param name="eventStore">The event store of the namespace.</param>
    /// <param name="issueId">The issue.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="GrantCouldNotLapse">The event log refused the lapse for another reason than a change on the issue.</exception>
    public async Task Lapse(IEventStore eventStore, IssueId issueId)
    {
        // The issue's own events decide, not the read model: they hold the grant's exact end, and they are what the
        // append is guarded against.
        var eventTypes = _accessEvents.Select(eventStore.EventTypes.GetEventTypeFor).ToArray();
        var accessEvents = await eventStore.EventLog.GetForEventSourceIdAndEventTypes(issueId, eventTypes);
        var last = accessEvents.Count > 0 ? accessEvents[^1] : null;
        if (last?.Content is not ReporterGrantedAccess granted || granted.ValidUntil.Value > time.GetUtcNow())
        {
            return;
        }

        var result = await eventStore.EventLog.AppendOnce(
            issueId,
            new AccessGrantLapsed(granted.ValidUntil),
            KeyFor(issueId, granted.ValidUntil),
            concurrencyScope: new ConcurrencyScope(last.Context.SequenceNumber, issueId, EventTypes: eventTypes));

        if (result.HasConcurrencyViolations)
        {
            // The reporter revoked or granted again between the read and the append; the next round decides afresh.
            logger.GrantChangedWhileLapsing(issueId, eventStore.Namespace);
            return;
        }

        if (!result.IsSuccess && !result.HasIdempotencyConflicts)
        {
            throw new GrantCouldNotLapse(issueId, eventStore.Namespace, result);
        }

        logger.GrantLapsed(issueId, granted.ValidUntil, eventStore.Namespace);
    }

    static IdempotencyKey KeyFor(IssueId issueId, ValidUntil validUntil) =>
        new($"usher-lapse-{issueId.Value:D}-{validUntil.Value.UtcTicks}");
}

/// <summary>
/// The exception that is thrown when the event log refuses the lapse of a grant.
/// </summary>
/// <param name="issueId">The issue the grant is on.</param>
/// <param name="namespace">The namespace of the issue.</param>
/// <param name="result">The result of the append.</param>
public class GrantCouldNotLapse(IssueId issueId, EventStoreNamespaceName @namespace, AppendResult result)
    : Exception($"The grant of access on issue '{issueId}' in namespace '{@namespace}' could not lapse: {result.Errors.Count()} error(s) and {result.ConstraintViolations.Count()} constraint violation(s).");

/// <summary>
/// A grant of access ended because its time passed without the reporter revoking it. The lapsing automation appends
/// it when <see cref="ValidUntil"/> passes; it is published as <c>AccessExpired</c>.
/// </summary>
/// <param name="ValidUntil">The end of the grant that lapsed, so a lapse of an earlier grant never ends a later one.</param>
[EventType]
public record AccessGrantLapsed(ValidUntil ValidUntil);
