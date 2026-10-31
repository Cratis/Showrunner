// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Receiving;

/// <summary>
/// Subscribes the application to the outbox of every operator, and observes the inbox the operator's events arrive in.
/// </summary>
/// <param name="translator">The <see cref="OperatorEventTranslator"/>.</param>
/// <param name="options">The <see cref="BoxOfficeOptions"/>.</param>
public class OperatorSubscriptions(OperatorEventTranslator translator, IOptions<BoxOfficeOptions> options)
{
    readonly Dictionary<IEventStore, HashSet<EventStoreName>> _served = new(ReferenceEqualityComparer.Instance);
    readonly HashSet<EventStoreName> _subscribed = [];
    readonly Lock _lock = new();

    /// <summary>
    /// Gets the identity of the translator for an operator store; its position in the inbox hangs on it.
    /// </summary>
    /// <param name="operatorStore">The operator store.</param>
    /// <returns>The reactor id.</returns>
    public static ReactorId TranslatorFor(EventStoreName operatorStore) => new($"Cratis.Showrunner.BoxOffice.Receiving.OperatorEventTranslator.{operatorStore.Value}");

    /// <summary>
    /// Gets the inbox the events of an operator store arrive in.
    /// </summary>
    /// <param name="operatorStore">The operator store.</param>
    /// <returns>The inbox.</returns>
    public static EventSequenceId InboxFor(EventStoreName operatorStore) => new($"{EventSequenceId.InboxPrefix}{operatorStore.Value}");

    /// <summary>
    /// Observes the inbox of every operator store in a namespace, unless it already does.
    /// </summary>
    /// <param name="eventStore">The event store of the namespace.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Observe(IEventStore eventStore)
    {
        foreach (var operatorStore in options.Value.OperatorStores)
        {
            lock (_lock)
            {
                if (_served.TryGetValue(eventStore, out var served) && served.Contains(operatorStore))
                {
                    continue;
                }
            }

            await eventStore.Reactors.Register(TranslatorFor(operatorStore), reactor => Translate(reactor, operatorStore));

            lock (_lock)
            {
                if (!_served.TryGetValue(eventStore, out var served))
                {
                    served = [];
                    _served[eventStore] = served;
                }

                served.Add(operatorStore);
            }
        }
    }

    /// <summary>
    /// Subscribes the application's event store to the outbox of every operator store, unless it already did.
    /// </summary>
    /// <param name="eventStore">The application's event store; a subscription covers all of its namespaces.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Subscribe(IEventStore eventStore)
    {
        foreach (var operatorStore in options.Value.OperatorStores)
        {
            lock (_lock)
            {
                if (_subscribed.Contains(operatorStore))
                {
                    continue;
                }
            }

            await eventStore.Subscriptions.Subscribe(
                new EventStoreSubscriptionId(operatorStore.Value),
                operatorStore.Value,
                subscription =>
                {
                    foreach (var eventType in OperatorDecisions.All)
                    {
                        subscription.WithEventType(eventStore.EventTypes.GetEventTypeFor(eventType).Id);
                    }
                });

            lock (_lock)
            {
                _subscribed.Add(operatorStore);
            }
        }
    }

    void Translate(IReactorBuilder reactor, EventStoreName operatorStore) => reactor
        .OnEventSequence(InboxFor(operatorStore))
        .NotReplayable()
        .On<TrialOpened>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<TrialEndMoved>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<TrialClosed>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<SeatCapacitySet>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<ModuleGranted>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<ModuleTrialGranted>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<ModulePreviewOpened>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<ModuleRevoked>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<AllowanceSet>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<TenantSuspended>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<TenantReinstated>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<OfferPublished>((@event, context) => translator.Translate(@event, context, operatorStore))
        .On<OfferWithdrawn>((@event, context) => translator.Translate(@event, context, operatorStore));
}
