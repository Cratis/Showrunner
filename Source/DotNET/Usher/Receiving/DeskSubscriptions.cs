// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Receiving;

/// <summary>
/// Connects the application to each configured desk store: subscribes the application's event store to the desk's
/// outbox, and observes the <c>inbox-{desk store}</c> sequence those events arrive in with a
/// <see cref="DeskEventTranslator"/>.
/// </summary>
/// <remarks>
/// <para>
/// The contracts name no store, so nothing routes them on its own: the subscription is explicit, one per desk store,
/// with the desk store's name as its id. Subscribing again with the same id keeps the existing subscription, so it is
/// safe on every start. Chronicle forwards a desk event into the inbox in the same namespace the desk published it in.
/// </para>
/// <para>
/// An observer runs per namespace, so the translator is registered in every namespace the application has. The
/// translator is not replayable: replaying an inbox would only translate events already recorded.
/// </para>
/// </remarks>
/// <param name="translator">The translator that records each desk event.</param>
/// <param name="options">Usher's options, which name the desk stores.</param>
public class DeskSubscriptions(DeskEventTranslator translator, IOptions<UsherOptions> options)
{
    readonly Dictionary<IEventStore, HashSet<EventStoreName>> _served = new(ReferenceEqualityComparer.Instance);
    readonly HashSet<EventStoreName> _subscribed = [];
    readonly Lock _lock = new();

    /// <summary>
    /// Gets the identity of the translator for a desk store; its position in the inbox hangs on it.
    /// </summary>
    /// <param name="deskStore">The desk store.</param>
    /// <returns>The reactor id.</returns>
    public static ReactorId TranslatorFor(EventStoreName deskStore) => new($"Cratis.Showrunner.Usher.Receiving.DeskEventTranslator.{deskStore.Value}");

    /// <summary>
    /// Gets the inbox the events of a desk store arrive in.
    /// </summary>
    /// <param name="deskStore">The desk store.</param>
    /// <returns>The inbox.</returns>
    public static EventSequenceId InboxFor(EventStoreName deskStore) => new($"{EventSequenceId.InboxPrefix}{deskStore.Value}");

    /// <summary>
    /// Observes the inbox of every desk store in a namespace, unless it already does.
    /// </summary>
    /// <param name="eventStore">The event store of the namespace.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Observe(IEventStore eventStore)
    {
        foreach (var deskStore in options.Value.DeskStores)
        {
            lock (_lock)
            {
                if (_served.TryGetValue(eventStore, out var served) && served.Contains(deskStore))
                {
                    continue;
                }
            }

            await eventStore.Reactors.Register(TranslatorFor(deskStore), reactor => Translate(reactor, deskStore));

            lock (_lock)
            {
                if (!_served.TryGetValue(eventStore, out var served))
                {
                    served = [];
                    _served[eventStore] = served;
                }

                served.Add(deskStore);
            }
        }
    }

    /// <summary>
    /// Subscribes the application's event store to the outbox of every desk store, unless it already did.
    /// </summary>
    /// <param name="eventStore">The application's event store; a subscription covers all of its namespaces.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Subscribe(IEventStore eventStore)
    {
        foreach (var deskStore in options.Value.DeskStores)
        {
            lock (_lock)
            {
                if (_subscribed.Contains(deskStore))
                {
                    continue;
                }
            }

            await eventStore.Subscriptions.Subscribe(
                new EventStoreSubscriptionId(deskStore.Value),
                deskStore.Value,
                subscription =>
                {
                    foreach (var eventType in PrivateEvents.FromDesk)
                    {
                        subscription.WithEventType(eventStore.EventTypes.GetEventTypeFor(eventType).Id);
                    }
                });

            lock (_lock)
            {
                _subscribed.Add(deskStore);
            }
        }
    }

    void Translate(IReactorBuilder reactor, EventStoreName deskStore) => reactor
        .OnEventSequence(InboxFor(deskStore))
        .NotReplayable()
        .On<IssueTriaged>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<IssueAccepted>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<IssueClosedAsWontFix>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<QuestionAsked>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<ReplySent>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<IssueLinkedToKnownIssue>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<IssueUnlinkedFromKnownIssue>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<IssueLinkedToWorkItem>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<IssueUnlinkedFromWorkItem>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<WorkStarted>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<IssueResolved>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<IssueFixedInRelease>((@event, context) => translator.Translate(@event, context, deskStore))
        .On<AccessRequested>((@event, context) => translator.Translate(@event, context, deskStore));
}
