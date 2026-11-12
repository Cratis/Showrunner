// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;

namespace Cratis.Showrunner.GuestList.Authoring.given;

/// <summary>
/// The guest list of a product built from decisions that happen one minute apart.
/// </summary>
public class a_product_guest_list : Specification
{
    protected ReadModelScenario<ProductGuestList> _scenario;
    ulong _sequenceNumber;

    void Establish() => _scenario = new();

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Gets what is held at the end.
    /// </summary>
    protected ProductGuestList GuestList => _scenario.Instance!;

    /// <summary>
    /// Lets decisions happen on the product, one minute apart, after any that happened before.
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
                new EventSourceId(Guests.Product.Value),
                EventStreamType.All,
                EventStreamId.Default,
                sequenceNumber,
                CorrelationId.New(),
                new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.Zero).AddMinutes(sequenceNumber));
            return new AppendedEvent(context, @event);
        })]);
}
#endif
