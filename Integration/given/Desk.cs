// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// A support desk: its own event store, which knows only the contracts, reads what the application publishes from
/// its inbox and publishes its answers in its outbox - the way a desk such as Direct does.
/// </summary>
/// <param name="client">The desk's Chronicle client.</param>
/// <param name="name">The name of the desk store.</param>
public sealed class Desk(IChronicleClient client, EventStoreName name) : IDisposable
{
    /// <summary>
    /// Every event the application publishes to the desk.
    /// </summary>
    public static readonly Type[] FromApplication =
    [
        typeof(ErrorReported),
        typeof(IssueReported),
        typeof(IssueAttributedToReporter),
        typeof(DetailsAdded),
        typeof(QuestionAnswered),
        typeof(IssueReopened),
        typeof(IssueWithdrawn),
        typeof(AccessGranted),
        typeof(AccessRevoked),
        typeof(AccessExpired),
    ];

    /// <summary>
    /// Gets the name of the desk store.
    /// </summary>
    public EventStoreName Name => name;

    /// <summary>
    /// Connects a desk with a unique store name.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <returns>The desk.</returns>
    /// <exception cref="InvalidOperationException">The desk store did not register.</exception>
    public static async Task<Desk> Connect(ChronicleInfrastructure infrastructure)
    {
        var name = new EventStoreName($"Direct-{Guid.NewGuid():N}");
        var client = new ChronicleClient(ChronicleOptions.FromConnectionString(infrastructure.ChronicleConnectionString), new ContractArtifacts());
        var store = await client.GetEventStore(name);
        var registration = await store.WaitForRegistration(TimeSpan.FromSeconds(30));
        if (!registration.IsSuccess)
        {
            throw new InvalidOperationException($"Desk store {name} did not register.", registration.Failure);
        }

        return new(client, name);
    }

    /// <summary>
    /// Serves a tenant: the desk store has the tenant's namespace, which is where Chronicle forwards what the
    /// application publishes in the same namespace.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The desk store in the tenant's namespace.</returns>
    public async Task<IEventStore> Serve(EventStoreNamespaceName tenant)
    {
        var store = await client.GetEventStore(name, tenant);
        await store.WaitForRegistration(TimeSpan.FromSeconds(30));
        return store;
    }

    /// <summary>
    /// Subscribes the desk store to the outbox of an application store.
    /// </summary>
    /// <param name="application">The application store.</param>
    /// <returns>An awaitable task.</returns>
    public async Task SubscribeTo(EventStoreName application)
    {
        var store = await client.GetEventStore(name, EventStoreNamespaceName.Default);
        await store.Subscriptions.Subscribe(
            new EventStoreSubscriptionId(application.Value),
            application.Value,
            subscription =>
            {
                foreach (var type in FromApplication)
                {
                    subscription.WithEventType(type.GetEventType().Id);
                }
            });
    }

    /// <summary>
    /// Publishes an event about an issue from the desk's default namespace.
    /// </summary>
    /// <param name="issueId">The issue.</param>
    /// <param name="event">The public event.</param>
    /// <returns>The event as the outbox holds it.</returns>
    /// <exception cref="InvalidOperationException">The outbox refused the event.</exception>
    public async Task<AppendedEvent> Publish(IssueId issueId, object @event)
    {
        var outbox = (await client.GetEventStore(name, EventStoreNamespaceName.Default)).GetEventSequence(EventSequenceId.Outbox);
        var result = await outbox.Append(issueId, @event);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Desk {name} failed to publish {@event.GetType().Name} for {issueId}.");
        }

        var published = await outbox.GetFromSequenceNumber(result.SequenceNumber, issueId);
        return published[0];
    }

    /// <summary>
    /// Waits for an event from an application in the inbox of a tenant namespace.
    /// </summary>
    /// <typeparam name="TEvent">The public event.</typeparam>
    /// <param name="application">The application store.</param>
    /// <param name="tenant">The tenant namespace.</param>
    /// <param name="issueId">The issue.</param>
    /// <returns>The event as the inbox holds it.</returns>
    public async Task<AppendedEvent> WaitFor<TEvent>(EventStoreName application, EventStoreNamespaceName tenant, IssueId issueId) =>
        await Eventually.Get(
            async () => (await ReceivedFrom(application, tenant, issueId)).FirstOrDefault(_ => _.Content is TEvent),
            $"{typeof(TEvent).Name} for {issueId} in {name}'s inbox from {application} in {tenant}");

    /// <summary>
    /// Gets everything received from an application about an issue in a tenant namespace.
    /// </summary>
    /// <param name="application">The application store.</param>
    /// <param name="tenant">The tenant namespace.</param>
    /// <param name="issueId">The issue.</param>
    /// <returns>The events, in the order they arrived.</returns>
    public async Task<IReadOnlyList<AppendedEvent>> ReceivedFrom(EventStoreName application, EventStoreNamespaceName tenant, IssueId issueId)
    {
        var store = await client.GetEventStore(name, tenant);
        var inbox = store.GetEventSequence(new EventSequenceId($"{EventSequenceId.InboxPrefix}{application.Value}"));
        return await inbox.GetForEventSourceIdAndEventTypes(issueId, FromApplication.Select(_ => _.GetEventType()));
    }

    /// <inheritdoc/>
    public void Dispose() => (client as IDisposable)?.Dispose();
}
