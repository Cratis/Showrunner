// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;
using ChronicleIdentity = Cratis.Chronicle.Identities.Identity;

namespace Cratis.Showrunner.Usher.given;

/// <summary>
/// A read model built from events that happen on an issue one minute apart, starting at <see cref="Start"/>.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
public class a_read_model<TReadModel> : Specification
    where TReadModel : class
{
    /// <summary>
    /// When the first event happens.
    /// </summary>
    protected static readonly DateTimeOffset Start = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);

    protected ReadModelScenario<TReadModel> _scenario;
    protected IssueId _issueId;
    ulong _sequenceNumber;

    void Establish()
    {
        _scenario = new();
        _issueId = IssueId.New();
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Gets when the event with a sequence number happened.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number, counting from zero.</param>
    /// <returns>When it happened.</returns>
    protected static DateTimeOffset At(ulong sequenceNumber) => Start.AddMinutes(sequenceNumber);

    /// <summary>
    /// Lets events happen on the issue, one minute apart, after any that happened before.
    /// </summary>
    /// <param name="events">The events, in the order they happen.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Happened(params object[] events) => HappenedOn(_issueId, events);

    /// <summary>
    /// Lets events happen on an issue, one minute apart, after any that happened before.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <param name="events">The events, in the order they happen.</param>
    /// <returns>An awaitable task.</returns>
    protected Task HappenedOn(IssueId issueId, params object[] events) => HappenedOn(issueId, null, events);

    protected Task HappenedCausedBy(ChronicleIdentity causedBy, params object[] events) => HappenedOn(_issueId, causedBy, events);

    protected Task HappenedOn(IssueId issueId, ChronicleIdentity? causedBy, params object[] events) =>
        _scenario.ProcessAppendedEvents([.. events.Select(@event =>
        {
            var sequenceNumber = _sequenceNumber++;
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                issueId,
                EventStreamType.All,
                EventStreamId.Default,
                sequenceNumber,
                CorrelationId.New(),
                At(sequenceNumber));
            return new AppendedEvent(causedBy is null ? context : context with { CausedBy = causedBy }, @event);
        })]);
}
#endif
