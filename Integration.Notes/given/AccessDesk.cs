// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Usher.Integration.given;

namespace Cratis.Showrunner.Notes.Integration.given;

/// <summary>
/// A desk that decides who may give feedback: its own event store, which knows only the contracts, and publishes its
/// decisions in its outbox - the way a desk such as Direct does.
/// </summary>
/// <param name="client">The desk's Chronicle client.</param>
/// <param name="name">The name of the desk store.</param>
public sealed class AccessDesk(IChronicleClient client, EventStoreName name) : IDisposable
{
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
    public static async Task<AccessDesk> Connect(ChronicleInfrastructure infrastructure)
    {
        var name = new EventStoreName($"Direct-{Guid.NewGuid():N}");
        var client = new ChronicleClient(ChronicleOptions.FromConnectionString(infrastructure.ChronicleConnectionString), new AccessContractArtifacts());
        var store = await client.GetEventStore(name);
        await infrastructure.WaitForRegistration(store, $"Desk store {name} did not register.", TimeSpan.FromSeconds(30));
        return new(client, name);
    }

    /// <summary>
    /// Grants a user the right to give feedback, as the desk's operator decides it.
    /// </summary>
    /// <param name="tenant">The tenant the user belongs to.</param>
    /// <param name="giver">The user.</param>
    /// <returns>An awaitable task.</returns>
    public Task Grant(TenantId tenant, FeedbackGiverIdentity giver) =>
        Publish(giver, new FeedbackAccessGranted(tenant, giver, "desk-operator-1"));

    /// <summary>
    /// Revokes a user's right to give feedback, as the desk's operator decides it.
    /// </summary>
    /// <param name="tenant">The tenant the user belongs to.</param>
    /// <param name="giver">The user.</param>
    /// <returns>An awaitable task.</returns>
    public Task Revoke(TenantId tenant, FeedbackGiverIdentity giver) =>
        Publish(giver, new FeedbackAccessRevoked(tenant, giver, "desk-operator-1"));

    /// <inheritdoc/>
    public void Dispose() => (client as IDisposable)?.Dispose();

    async Task Publish(FeedbackGiverIdentity giver, object decision)
    {
        var outbox = (await client.GetEventStore(name, EventStoreNamespaceName.Default)).GetEventSequence(EventSequenceId.Outbox);

        // The desk records each decision about a user under the user's own id, so a grant and a later revocation of one
        // user are delivered in the order the desk made them.
        var result = await outbox.Append(FeedbackGiverId.For(giver), decision, subject: FeedbackGiverId.SubjectOf(giver));
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Desk {name} failed to publish {decision.GetType().Name}.");
        }
    }
}
