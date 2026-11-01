// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Notes.Contracts.Responses;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Receiving;

/// <summary>
/// Connects the application to each configured collecting store: subscribes the application's event store to the
/// collecting store's outbox, and observes the <c>inbox-{collecting store}</c> sequence those events arrive in with a
/// <see cref="ReceiverEventTranslator"/>.
/// </summary>
/// <remarks>
/// <para>
/// The contracts name no store, so nothing routes them on its own: the subscription is explicit, one per collecting
/// store. Its id is the collecting store's name prefixed with <c>notes-</c>, so it does not collide with a subscription to
/// the same store that another part, such as Usher, makes for its own events; both arrive in the same inbox and each
/// reactor handles only its own. Subscribing again with the same id keeps the existing subscription, so it is safe on
/// every start. Chronicle forwards an event into the inbox in the same namespace the collecting store published it in.
/// </para>
/// <para>
/// An observer runs per namespace, so the translator is registered in every namespace the application has. The
/// translator is not replayable: replaying an inbox would only translate events already recorded.
/// </para>
/// </remarks>
/// <param name="translator">The translator that records each event from the receiving side.</param>
/// <param name="options">Notes' options, which name the collecting stores.</param>
public class ReceiverSubscriptions(ReceiverEventTranslator translator, IOptions<NotesOptions> options)
{
    readonly Dictionary<IEventStore, HashSet<EventStoreName>> _served = new(ReferenceEqualityComparer.Instance);
    readonly HashSet<EventStoreName> _subscribed = [];
    readonly Lock _lock = new();

    /// <summary>
    /// Gets the identity of the translator for a collecting store; its position in the inbox hangs on it.
    /// </summary>
    /// <param name="collectingStore">The collecting store.</param>
    /// <returns>The reactor id.</returns>
    public static ReactorId TranslatorFor(EventStoreName collectingStore) => new($"Cratis.Showrunner.Notes.Receiving.ReceiverEventTranslator.{collectingStore.Value}");

    /// <summary>
    /// Gets the inbox the events of a collecting store arrive in.
    /// </summary>
    /// <param name="collectingStore">The collecting store.</param>
    /// <returns>The inbox.</returns>
    public static EventSequenceId InboxFor(EventStoreName collectingStore) => new($"{EventSequenceId.InboxPrefix}{collectingStore.Value}");

    /// <summary>
    /// Gets the identity of the subscription to a collecting store.
    /// </summary>
    /// <param name="collectingStore">The collecting store.</param>
    /// <returns>The subscription id.</returns>
    public static EventStoreSubscriptionId SubscriptionFor(EventStoreName collectingStore) => new($"notes-{collectingStore.Value}");

    /// <summary>
    /// Observes the inbox of every collecting store in a namespace, unless it already does.
    /// </summary>
    /// <param name="eventStore">The event store of the namespace.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Observe(IEventStore eventStore)
    {
        foreach (var collectingStore in options.Value.CollectingStores)
        {
            lock (_lock)
            {
                if (_served.TryGetValue(eventStore, out var served) && served.Contains(collectingStore))
                {
                    continue;
                }
            }

            await eventStore.Reactors.Register(TranslatorFor(collectingStore), reactor => Translate(reactor, collectingStore));

            lock (_lock)
            {
                if (!_served.TryGetValue(eventStore, out var served))
                {
                    served = [];
                    _served[eventStore] = served;
                }

                served.Add(collectingStore);
            }
        }
    }

    /// <summary>
    /// Subscribes the application's event store to the outbox of every collecting store, unless it already did.
    /// </summary>
    /// <param name="eventStore">The application's event store; a subscription covers all of its namespaces.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Subscribe(IEventStore eventStore)
    {
        foreach (var collectingStore in options.Value.CollectingStores)
        {
            lock (_lock)
            {
                if (_subscribed.Contains(collectingStore))
                {
                    continue;
                }
            }

            await eventStore.Subscriptions.Subscribe(
                SubscriptionFor(collectingStore),
                collectingStore.Value,
                subscription =>
                {
                    foreach (var eventType in PrivateEvents.FromReceiver)
                    {
                        subscription.WithEventType(eventStore.EventTypes.GetEventTypeFor(eventType).Id);
                    }
                });

            lock (_lock)
            {
                _subscribed.Add(collectingStore);
            }
        }
    }

    void Translate(IReactorBuilder reactor, EventStoreName collectingStore) => reactor
        .OnEventSequence(InboxFor(collectingStore))
        .NotReplayable()
        .On<NoteAcknowledged>((@event, context) => translator.Translate(@event, context, collectingStore))
        .On<NoteAddressed>((@event, context) => translator.Translate(@event, context, collectingStore))
        .On<NoteDeclined>((@event, context) => translator.Translate(@event, context, collectingStore));
}
