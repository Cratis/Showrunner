// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;

namespace Cratis.Showrunner.Playbill.given;

/// <summary>
/// A read model built from events that happen on my stream one minute apart.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
public class a_read_model<TReadModel> : Specification
    where TReadModel : class
{
    /// <summary>
    /// When the first event happens.
    /// </summary>
    protected static readonly DateTimeOffset Start = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    protected ReadModelScenario<TReadModel> _scenario;
    ulong _sequenceNumber;

    void Establish() => _scenario = new();

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Lets events happen on my stream, one minute apart, after any that happened before.
    /// </summary>
    /// <param name="events">The events, in the order they happen.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Happened(params object[] events) =>
        _scenario.ProcessAppendedEvents([.. events.Select(@event =>
        {
            var sequenceNumber = _sequenceNumber++;
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                ViewerId.For(People.Me),
                EventStreamType.All,
                EventStreamId.Default,
                sequenceNumber,
                CorrelationId.New(),
                Start.AddMinutes(sequenceNumber));
            return new AppendedEvent(context, @event);
        })]);
}
#endif
