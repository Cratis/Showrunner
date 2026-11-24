// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Querying;

/// <summary>
/// Represents an implementation of <see cref="IPartSuspensions"/> that reads the read models the product keeps: the tenant's
/// and its users' in the tenant's own namespace, the system's in the product's own.
/// </summary>
/// <param name="client">The Chronicle client.</param>
/// <param name="options">The Chronicle client options, which name the product's event store.</param>
public class PartSuspensions(IChronicleClient client, IOptions<ChronicleClientOptions> options) : IPartSuspensions
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PartDecision>> ForSystem(CancellationToken cancellationToken = default) =>
        Decided(await Read(EventStoreNamespaceName.Default, GuestListStreams.System, cancellationToken));

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PartDecision>> ForTenant(TenantId tenant, CancellationToken cancellationToken = default) =>
        Decided(await Read(new EventStoreNamespaceName(tenant.Value), GuestListStreams.Tenant, cancellationToken));

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PartDecision>> ForUser(TenantId tenant, UserId user, CancellationToken cancellationToken = default) =>
        Decided(await Read(new EventStoreNamespaceName(tenant.Value), GuestListStreams.User(user), cancellationToken));

    static IReadOnlyList<PartDecision> Decided(SuspensionHoldings? holdings) =>
        holdings is null ? [] : [.. holdings.Decisions.OrderBy(decision => decision.Part.Value, StringComparer.Ordinal)];

    async Task<SuspensionHoldings?> Read(EventStoreNamespaceName @namespace, EventSourceId id, CancellationToken cancellationToken)
    {
        var store = await client.GetEventStore(options.Value.EventStore, @namespace).WaitAsync(cancellationToken);
        return await store.ReadModels.GetInstanceById<SuspensionHoldings>(new ReadModelKey(id.Value));
    }
}
