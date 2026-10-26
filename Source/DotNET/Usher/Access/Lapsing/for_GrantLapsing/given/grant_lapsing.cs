// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.given;

/// <summary>
/// <see cref="GrantLapsing"/> over an in-process event store, with the clock at <see cref="Now"/>.
/// </summary>
public class grant_lapsing : Specification
{
    /// <summary>
    /// The time the clock is at.
    /// </summary>
    protected static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    protected EventStoreForTesting _eventStore;
    protected ScenarioTime _time;
    protected RecordingLogger<GrantLapsing> _logger;
    protected GrantLapsing _lapsing;
    protected IssueId _issueId;
    protected IImmutableList<AppendedEvent> _events;

    void Establish()
    {
        _eventStore = new();
        _time = new(Now);
        _logger = new();
        _lapsing = new(_time, _logger);
        _issueId = IssueId.New();
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Lets events happen on an issue, after it was reported.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <param name="events">The events, in the order they happened.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task HappenedOn(IssueId issueId, params object[] events)
    {
        await _eventStore.EventLog.Append(issueId, IssueEvents.Described(Reporters.Subject));
        foreach (var @event in events)
        {
            await _eventStore.EventLog.Append(issueId, @event);
        }
    }

    /// <summary>
    /// Reads what happened on an issue.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadEventsOf(IssueId issueId) =>
        _events = await _eventStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, issueId);
}
#endif
