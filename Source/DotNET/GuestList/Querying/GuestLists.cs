// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Querying;

/// <summary>
/// Represents an implementation of <see cref="IGuestList"/> that reads the read models the product keeps: the tenant's and
/// its users' in the tenant's own namespace, the system's in the product's own.
/// </summary>
/// <param name="client">The Chronicle client.</param>
/// <param name="options">The Chronicle client options, which name the product's event store.</param>
public class GuestLists(IChronicleClient client, IOptions<ChronicleClientOptions> options) : IGuestList
{
    const string UserPrefix = "user:";

    /// <inheritdoc/>
    public async Task<IReadOnlySet<AudienceName>> ForSystem(CancellationToken cancellationToken = default) =>
        Held(await Read(EventStoreNamespaceName.Default, GuestListStreams.System, cancellationToken));

    /// <inheritdoc/>
    public async Task<IReadOnlySet<AudienceName>> ForTenant(TenantId tenant, CancellationToken cancellationToken = default) =>
        Held(await Read(new EventStoreNamespaceName(tenant.Value), GuestListStreams.Tenant, cancellationToken));

    /// <inheritdoc/>
    public async Task<IReadOnlySet<AudienceName>> ForUser(TenantId tenant, UserId user, CancellationToken cancellationToken = default) =>
        Held(await Read(new EventStoreNamespaceName(tenant.Value), GuestListStreams.User(user), cancellationToken));

    /// <inheritdoc/>
    public async Task<IReadOnlySet<AudienceName>> For(TenantId tenant, UserId user, CancellationToken cancellationToken = default)
    {
        HashSet<AudienceName> audiences = [.. await ForSystem(cancellationToken), .. await ForTenant(tenant, cancellationToken)];
        if (!string.IsNullOrWhiteSpace(user.Value))
        {
            audiences.UnionWith(await ForUser(tenant, user, cancellationToken));
        }

        return audiences;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<GrantedAudience>> Granted(TenantId tenant, CancellationToken cancellationToken = default)
    {
        var store = await Store(new EventStoreNamespaceName(tenant.Value), cancellationToken);
        var holdings = await store.ReadModels.GetInstances<AudienceHoldings>();
        List<GrantedAudience> granted = [];
        foreach (var held in holdings)
        {
            var user = held.Level == AudienceLevel.User && held.Id.StartsWith(UserPrefix, StringComparison.Ordinal)
                ? new UserId(held.Id[UserPrefix.Length..])
                : UserId.NotSet;
            granted.AddRange(held.HeldAudiences().Select(audience => new GrantedAudience(held.Level, user, audience)));
        }

        granted.AddRange((await ForSystem(cancellationToken)).Select(audience => new GrantedAudience(AudienceLevel.System, UserId.NotSet, audience)));
        return
        [
            .. granted
                .OrderBy(grant => grant.Level)
                .ThenBy(grant => grant.UserId.Value, StringComparer.Ordinal)
                .ThenBy(grant => grant.Audience.Value, StringComparer.Ordinal)
        ];
    }

    static HashSet<AudienceName> Held(AudienceHoldings? holdings) => holdings is null ? [] : [.. holdings.HeldAudiences()];

    async Task<IEventStore> Store(EventStoreNamespaceName @namespace, CancellationToken cancellationToken) =>
        await client.GetEventStore(options.Value.EventStore, @namespace).WaitAsync(cancellationToken);

    async Task<AudienceHoldings?> Read(EventStoreNamespaceName @namespace, EventSourceId id, CancellationToken cancellationToken)
    {
        var store = await Store(@namespace, cancellationToken);
        return await store.ReadModels.GetInstanceById<AudienceHoldings>(new ReadModelKey(id.Value));
    }
}
