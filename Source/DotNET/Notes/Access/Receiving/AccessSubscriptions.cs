// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.Receiving;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Access.Receiving;

/// <summary>
/// Connects the application to each configured access store, but only when <see cref="NotesOptions.AccessMode"/> is
/// <see cref="FeedbackAccessMode.GrantedUsers"/>: subscribes the application's event store to the access store's outbox, and
/// observes the <c>inbox-{access store}</c> sequence those events arrive in with an <see cref="AccessEventTranslator"/>.
/// </summary>
/// <remarks>
/// <para>
/// It mirrors <see cref="ReceiverSubscriptions"/>. The subscription id is the access store's name prefixed with
/// <c>notes-access-</c>, so it does not collide with the <c>notes-</c> subscription to the same store when one store both
/// collects notes and decides access; both arrive in the same inbox and each reactor handles only its own events.
/// Subscribing again with the same id keeps the existing subscription, so it is safe on every start.
/// </para>
/// <para>
/// An observer runs per namespace, so the translator is registered in every namespace the application has. The translator is
/// not replayable: replaying an inbox would only translate events already recorded.
/// </para>
/// </remarks>
/// <param name="translator">The translator that records each event from a desk.</param>
/// <param name="options">Notes' options, which name the access stores.</param>
public class AccessSubscriptions(AccessEventTranslator translator, IOptions<NotesOptions> options)
{
    readonly Dictionary<IEventStore, HashSet<EventStoreName>> _served = [with(ReferenceEqualityComparer.Instance)];
    readonly HashSet<EventStoreName> _subscribed = [];
    readonly Lock _lock = new();

    /// <summary>
    /// Gets the identity of the translator for an access store; its position in the inbox hangs on it.
    /// </summary>
    /// <param name="accessStore">The access store.</param>
    /// <returns>The reactor id.</returns>
    public static ReactorId TranslatorFor(EventStoreName accessStore) => new($"Cratis.Showrunner.Notes.Access.Receiving.AccessEventTranslator.{accessStore.Value}");

    /// <summary>
    /// Gets the inbox the events of an access store arrive in.
    /// </summary>
    /// <param name="accessStore">The access store.</param>
    /// <returns>The inbox.</returns>
    public static EventSequenceId InboxFor(EventStoreName accessStore) => new($"{EventSequenceId.InboxPrefix}{accessStore.Value}");

    /// <summary>
    /// Gets the identity of the subscription to an access store.
    /// </summary>
    /// <param name="accessStore">The access store.</param>
    /// <returns>The subscription id.</returns>
    public static EventStoreSubscriptionId SubscriptionFor(EventStoreName accessStore) => new($"notes-access-{accessStore.Value}");

    /// <summary>
    /// Observes the inbox of every access store in a namespace, unless it already does or the application lets everyone give feedback.
    /// </summary>
    /// <param name="eventStore">The event store of the namespace.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Observe(IEventStore eventStore)
    {
        if (options.Value.AccessMode != FeedbackAccessMode.GrantedUsers)
        {
            return;
        }

        foreach (var accessStore in options.Value.AccessStores)
        {
            lock (_lock)
            {
                if (_served.TryGetValue(eventStore, out var served) && served.Contains(accessStore))
                {
                    continue;
                }
            }

            await eventStore.Reactors.Register(TranslatorFor(accessStore), reactor => Translate(reactor, accessStore));

            lock (_lock)
            {
                if (!_served.TryGetValue(eventStore, out var served))
                {
                    served = [];
                    _served[eventStore] = served;
                }

                served.Add(accessStore);
            }
        }
    }

    /// <summary>
    /// Subscribes the application's event store to the outbox of every access store, unless it already did or the application
    /// lets everyone give feedback.
    /// </summary>
    /// <param name="eventStore">The application's event store; a subscription covers all of its namespaces.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Subscribe(IEventStore eventStore)
    {
        if (options.Value.AccessMode != FeedbackAccessMode.GrantedUsers)
        {
            return;
        }

        foreach (var accessStore in options.Value.AccessStores)
        {
            lock (_lock)
            {
                if (_subscribed.Contains(accessStore))
                {
                    continue;
                }
            }

            await eventStore.Subscriptions.Subscribe(
                SubscriptionFor(accessStore),
                accessStore.Value,
                subscription =>
                {
                    foreach (var eventType in AccessPrivateEvents.FromDesk)
                    {
                        subscription.WithEventType(eventStore.EventTypes.GetEventTypeFor(eventType).Id);
                    }

                    ReceiverSubscriptions.FilterByAudience(subscription, options.Value.Audience);
                });

            lock (_lock)
            {
                _subscribed.Add(accessStore);
            }
        }
    }

    void Translate(IReactorBuilder reactor, EventStoreName accessStore) => reactor
        .OnEventSequence(InboxFor(accessStore))
        .NotReplayable()
        .On<FeedbackAccessGranted>((@event, context) => translator.Translate(@event, context, accessStore))
        .On<FeedbackAccessRevoked>((@event, context) => translator.Translate(@event, context, accessStore));
}
