// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;

namespace Cratis.Showrunner.BoxOffice.Catalog.given;

/// <summary>
/// A read model scenario for the offer read model, where events happen on an offer one minute apart.
/// </summary>
public class an_offer_read_model : Specification
{
    /// <summary>
    /// When the first event happens.
    /// </summary>
    protected static readonly DateTimeOffset Start = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    protected ReadModelScenario<Offer> _scenario;
    protected OfferId _offerId;
    ulong _sequenceNumber;

    void Establish()
    {
        _scenario = new();
        _offerId = OfferId.New();
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Lets events happen on the offer, after any that happened before.
    /// </summary>
    /// <param name="events">The events, in the order they happen; arrays of events are flattened.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Happened(params object[] events) => HappenedOn(_offerId, events);

    /// <summary>
    /// Lets events happen on an offer, after any that happened before.
    /// </summary>
    /// <param name="offerId">The offer.</param>
    /// <param name="events">The events, in the order they happen; arrays of events are flattened.</param>
    /// <returns>An awaitable task.</returns>
    protected Task HappenedOn(OfferId offerId, params object[] events) =>
        _scenario.ProcessAppendedEvents([.. OfferEvents.Then(events).Select(@event =>
        {
            var sequenceNumber = _sequenceNumber++;
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                offerId,
                EventStreamType.All,
                EventStreamId.Default,
                sequenceNumber,
                CorrelationId.New(),
                Start.AddMinutes(sequenceNumber));
            return new AppendedEvent(context, @event);
        })]);
}
#endif
