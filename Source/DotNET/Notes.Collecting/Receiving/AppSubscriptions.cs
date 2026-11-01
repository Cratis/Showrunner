// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// Connects the collecting store to each application store: subscribes it to the application store's outbox, and observes
/// the <c>inbox-{application store}</c> sequence those events arrive in with an <see cref="AppEventTranslator"/>.
/// </summary>
/// <remarks>
/// <para>
/// The contracts name no store, so nothing routes them on its own: the subscription is explicit, one per application
/// store, with the application store's name prefixed with <c>notes-</c> as its id. Subscribing again with the same id keeps
/// the existing subscription, so it is safe on every start. Chronicle forwards an event into the inbox in the same
/// namespace the application published it in, and only into namespaces the collecting store has.
/// </para>
/// <para>
/// An observer runs per namespace, so the translator is registered in every namespace the collecting store has. The
/// translator is not replayable: replaying an inbox would only translate events already recorded.
/// </para>
/// </remarks>
/// <param name="translator">The translator that records each event from an application.</param>
/// <param name="stores">The application stores to read notes from.</param>
public class AppSubscriptions(AppEventTranslator translator, IAppStores stores)
{
    readonly Dictionary<IEventStore, HashSet<EventStoreName>> _served = new(ReferenceEqualityComparer.Instance);
    readonly HashSet<EventStoreName> _subscribed = [];
    readonly Lock _lock = new();

    /// <summary>
    /// Gets the identity of the translator for an application store; its position in the inbox hangs on it.
    /// </summary>
    /// <param name="appStore">The application store.</param>
    /// <returns>The reactor id.</returns>
    public static ReactorId TranslatorFor(EventStoreName appStore) => new($"Cratis.Showrunner.Notes.Collecting.Receiving.AppEventTranslator.{appStore.Value}");

    /// <summary>
    /// Gets the inbox the events of an application store arrive in.
    /// </summary>
    /// <param name="appStore">The application store.</param>
    /// <returns>The inbox.</returns>
    public static EventSequenceId InboxFor(EventStoreName appStore) => new($"{EventSequenceId.InboxPrefix}{appStore.Value}");

    /// <summary>
    /// Gets the identity of the subscription to an application store.
    /// </summary>
    /// <param name="appStore">The application store.</param>
    /// <returns>The subscription id.</returns>
    public static EventStoreSubscriptionId SubscriptionFor(EventStoreName appStore) => new($"notes-{appStore.Value}");

    /// <summary>
    /// Observes the inbox of every application store in a namespace, unless it already does.
    /// </summary>
    /// <param name="eventStore">The collecting store in the namespace.</param>
    /// <param name="cancellationToken">Stops waiting when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Observe(IEventStore eventStore, CancellationToken cancellationToken = default)
    {
        foreach (var appStore in await stores.Get(cancellationToken))
        {
            lock (_lock)
            {
                if (_served.TryGetValue(eventStore, out var served) && served.Contains(appStore))
                {
                    continue;
                }
            }

            await eventStore.Reactors.Register(TranslatorFor(appStore), reactor => Translate(reactor, appStore));

            lock (_lock)
            {
                if (!_served.TryGetValue(eventStore, out var served))
                {
                    served = [];
                    _served[eventStore] = served;
                }

                served.Add(appStore);
            }
        }
    }

    /// <summary>
    /// Subscribes the collecting store to the outbox of every application store, unless it already did.
    /// </summary>
    /// <param name="eventStore">The collecting store; a subscription covers all of its namespaces.</param>
    /// <param name="cancellationToken">Stops waiting when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Subscribe(IEventStore eventStore, CancellationToken cancellationToken = default)
    {
        foreach (var appStore in await stores.Get(cancellationToken))
        {
            lock (_lock)
            {
                if (_subscribed.Contains(appStore))
                {
                    continue;
                }
            }

            await eventStore.Subscriptions.Subscribe(
                SubscriptionFor(appStore),
                appStore.Value,
                subscription =>
                {
                    foreach (var eventType in PrivateEvents.FromApplications)
                    {
                        subscription.WithEventType(eventStore.EventTypes.GetEventTypeFor(eventType).Id);
                    }
                });

            lock (_lock)
            {
                _subscribed.Add(appStore);
            }
        }
    }

    void Translate(IReactorBuilder reactor, EventStoreName appStore) => reactor
        .OnEventSequence(InboxFor(appStore))
        .NotReplayable()
        .On<NoteOnContextGiven>((@event, context) => translator.Translate(@event, context, appStore))
        .On<NoteOnSlicesGiven>((@event, context) => translator.Translate(@event, context, appStore))
        .On<NoteOnScreenGiven>((@event, context) => translator.Translate(@event, context, appStore))
        .On<NoteOnComponentGiven>((@event, context) => translator.Translate(@event, context, appStore))
        .On<ComponentPropertyChangeSuggested>((@event, context) => translator.Translate(@event, context, appStore))
        .On<NoteWithdrawn>((@event, context) => translator.Translate(@event, context, appStore));
}
