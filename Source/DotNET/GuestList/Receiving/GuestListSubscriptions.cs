// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Receiving;

/// <summary>
/// Connects the product to each configured operator store: subscribes the product's event store to the operator store's
/// outbox, and observes the <c>inbox-{operator store}</c> sequence those events arrive in with a <see cref="GuestListInbox"/>.
/// </summary>
/// <remarks>
/// <para>
/// The contracts name no store, so nothing routes them on its own: the subscription is explicit, one per operator store. Its
/// id is the operator store's name prefixed with <c>guestlist-</c>, so it does not collide with a subscription to the same
/// store that another part, such as Marquee or Box Office, makes for its own events; they arrive in the same inbox and each
/// reactor handles only its own. Subscribing again with the same id keeps the existing subscription, so it is safe on
/// every start.
/// </para>
/// <para>
/// The operator publishes in its own default namespace, and Chronicle forwards an event into the inbox of the same
/// namespace, so the inbox is observed in the product's own (default) namespace only. The inbox is not replayable: replaying
/// it would only receive again what is already recorded.
/// </para>
/// </remarks>
/// <param name="inbox">The inbox that records each decision.</param>
/// <param name="options">GuestList's options, which name the operator stores.</param>
public class GuestListSubscriptions(GuestListInbox inbox, IOptions<GuestListOptions> options)
{
    readonly HashSet<EventStoreName> _observed = [];
    readonly HashSet<EventStoreName> _subscribed = [];
    readonly Lock _lock = new();

    /// <summary>
    /// Gets the public events the product receives, and subscribes to.
    /// </summary>
    public static IReadOnlyList<Type> PublicEvents { get; } =
    [
        typeof(AudienceGranted),
        typeof(AudienceWithdrawn)
    ];

    /// <summary>
    /// Gets the identity of the inbox reactor for an operator store; its position in the inbox hangs on it.
    /// </summary>
    /// <param name="operatorStore">The operator store.</param>
    /// <returns>The reactor id.</returns>
    public static ReactorId InboxFor(EventStoreName operatorStore) => new($"Cratis.Showrunner.GuestList.Receiving.GuestListInbox.{operatorStore.Value}");

    /// <summary>
    /// Gets the inbox the events of an operator store arrive in.
    /// </summary>
    /// <param name="operatorStore">The operator store.</param>
    /// <returns>The inbox.</returns>
    public static EventSequenceId SequenceFor(EventStoreName operatorStore) => new($"{EventSequenceId.InboxPrefix}{operatorStore.Value}");

    /// <summary>
    /// Gets the identity of the subscription to an operator store.
    /// </summary>
    /// <param name="operatorStore">The operator store.</param>
    /// <returns>The subscription id.</returns>
    public static EventStoreSubscriptionId SubscriptionFor(EventStoreName operatorStore) => new($"guestlist-{operatorStore.Value}");

    /// <summary>
    /// Narrows a subscription to the events addressed to this product, when it has a routing audience.
    /// </summary>
    /// <param name="subscription">The subscription being defined.</param>
    /// <param name="routingAudience">The product's routing audience; <see langword="null"/> or blank leaves the subscription unfiltered.</param>
    public static void FilterByRoutingAudience(IEventStoreSubscriptionBuilder subscription, string? routingAudience)
    {
        if (!string.IsNullOrWhiteSpace(routingAudience))
        {
            subscription.ForAudience(routingAudience);
        }
    }

    /// <summary>
    /// Observes the inbox of every operator store, unless it already does.
    /// </summary>
    /// <param name="eventStore">The event store in the product's own namespace.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Observe(IEventStore eventStore)
    {
        foreach (var operatorStore in options.Value.OperatorStores)
        {
            lock (_lock)
            {
                if (_observed.Contains(operatorStore))
                {
                    continue;
                }
            }

            await eventStore.Reactors.Register(InboxFor(operatorStore), reactor => Receive(reactor, operatorStore));

            lock (_lock)
            {
                _observed.Add(operatorStore);
            }
        }
    }

    /// <summary>
    /// Subscribes the product's event store to the outbox of every operator store, unless it already did.
    /// </summary>
    /// <param name="eventStore">The product's event store.</param>
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
                SubscriptionFor(operatorStore),
                operatorStore.Value,
                subscription =>
                {
                    foreach (var eventType in PublicEvents)
                    {
                        subscription.WithEventType(eventStore.EventTypes.GetEventTypeFor(eventType).Id);
                    }

                    FilterByRoutingAudience(subscription, options.Value.SubscribeForProduct ? options.Value.Product.Value : null);
                });

            lock (_lock)
            {
                _subscribed.Add(operatorStore);
            }
        }
    }

    void Receive(IReactorBuilder reactor, EventStoreName operatorStore) => reactor
        .OnEventSequence(SequenceFor(operatorStore))
        .NotReplayable()
        .On<AudienceGranted>((@event, context) => inbox.Receive(@event, context, operatorStore))
        .On<AudienceWithdrawn>((@event, context) => inbox.Receive(@event, context, operatorStore));
}
