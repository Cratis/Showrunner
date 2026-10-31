// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Marquee.Receiving;

/// <summary>
/// Connects the product to each configured authoring store: subscribes the product's event store to the authoring
/// store's outbox, and observes the <c>inbox-{authoring store}</c> sequence those events arrive in with a
/// <see cref="MarqueeInbox"/>.
/// </summary>
/// <remarks>
/// <para>
/// The contracts name no store, so nothing routes them on its own: the subscription is explicit, one per authoring
/// store. Its id is the authoring store's name prefixed with <c>marquee-</c>, so it does not collide with a subscription
/// to the same store that another part, such as Usher, makes for its own events; both arrive in the same inbox and each
/// reactor handles only its own. Subscribing again with the same id keeps the existing subscription, so it is safe on
/// every start.
/// </para>
/// <para>
/// The announcements are the same for every tenant, so the inbox is observed in one namespace only, the product's own
/// (<see cref="MarqueeStream.Namespace"/>). Chronicle forwards an event into the inbox of the namespace the authoring
/// store published it in; the authoring store publishes in its own default namespace. The inbox is not replayable:
/// replaying it would only receive again what is already recorded.
/// </para>
/// </remarks>
/// <param name="inbox">The inbox that records each announcement event.</param>
/// <param name="options">Marquee's options, which name the authoring stores.</param>
public class MarqueeSubscriptions(MarqueeInbox inbox, IOptions<MarqueeOptions> options)
{
    readonly HashSet<EventStoreName> _observed = [];
    readonly HashSet<EventStoreName> _subscribed = [];
    readonly Lock _lock = new();

    /// <summary>
    /// Gets the public events the product receives, and subscribes to.
    /// </summary>
    public static IReadOnlyList<Type> PublicEvents { get; } =
    [
        typeof(AnnouncementPublished),
        typeof(AnnouncementRevised),
        typeof(AnnouncementAudienceChanged),
        typeof(AnnouncementRetracted)
    ];

    /// <summary>
    /// Gets the identity of the inbox reactor for an authoring store; its position in the inbox hangs on it.
    /// </summary>
    /// <param name="authoringStore">The authoring store.</param>
    /// <returns>The reactor id.</returns>
    public static ReactorId InboxFor(EventStoreName authoringStore) => new($"Cratis.Showrunner.Marquee.Receiving.MarqueeInbox.{authoringStore.Value}");

    /// <summary>
    /// Gets the inbox the events of an authoring store arrive in.
    /// </summary>
    /// <param name="authoringStore">The authoring store.</param>
    /// <returns>The inbox.</returns>
    public static EventSequenceId SequenceFor(EventStoreName authoringStore) => new($"{EventSequenceId.InboxPrefix}{authoringStore.Value}");

    /// <summary>
    /// Gets the identity of the subscription to an authoring store.
    /// </summary>
    /// <param name="authoringStore">The authoring store.</param>
    /// <returns>The subscription id.</returns>
    public static EventStoreSubscriptionId SubscriptionFor(EventStoreName authoringStore) => new($"marquee-{authoringStore.Value}");

    /// <summary>
    /// Observes the inbox of every authoring store, unless it already does.
    /// </summary>
    /// <param name="eventStore">The event store in the product's own namespace.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Observe(IEventStore eventStore)
    {
        foreach (var authoringStore in options.Value.AuthoringStores)
        {
            lock (_lock)
            {
                if (_observed.Contains(authoringStore))
                {
                    continue;
                }
            }

            await eventStore.Reactors.Register(InboxFor(authoringStore), reactor => Receive(reactor, authoringStore));

            lock (_lock)
            {
                _observed.Add(authoringStore);
            }
        }
    }

    /// <summary>
    /// Subscribes the product's event store to the outbox of every authoring store, unless it already did.
    /// </summary>
    /// <param name="eventStore">The product's event store.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Subscribe(IEventStore eventStore)
    {
        foreach (var authoringStore in options.Value.AuthoringStores)
        {
            lock (_lock)
            {
                if (_subscribed.Contains(authoringStore))
                {
                    continue;
                }
            }

            await eventStore.Subscriptions.Subscribe(
                SubscriptionFor(authoringStore),
                authoringStore.Value,
                subscription =>
                {
                    foreach (var eventType in PublicEvents)
                    {
                        subscription.WithEventType(eventStore.EventTypes.GetEventTypeFor(eventType).Id);
                    }
                });

            lock (_lock)
            {
                _subscribed.Add(authoringStore);
            }
        }
    }

    void Receive(IReactorBuilder reactor, EventStoreName authoringStore) => reactor
        .OnEventSequence(SequenceFor(authoringStore))
        .NotReplayable()
        .On<AnnouncementPublished>((@event, context) => inbox.Receive(@event, context, authoringStore))
        .On<AnnouncementRevised>((@event, context) => inbox.Receive(@event, context, authoringStore))
        .On<AnnouncementAudienceChanged>((@event, context) => inbox.Receive(@event, context, authoringStore))
        .On<AnnouncementRetracted>((@event, context) => inbox.Receive(@event, context, authoringStore));
}
