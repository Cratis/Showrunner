// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Tenancy;

/// <summary>
/// Opens the application's event store in every tenant namespace it has.
/// </summary>
/// <remarks>
/// Chronicle runs the application's reactors and reducers per namespace, and only in the namespaces the application
/// has opened. A web request opens the namespace of its tenant, but Usher has work to do in a tenant also when nobody
/// from it is signed in: publishing what was recorded before a restart, receiving the desk's answers and lapsing grants.
/// Opening a namespace registers Usher's observers in it.
/// </remarks>
/// <param name="client">The Chronicle client.</param>
/// <param name="options">The Chronicle client options, which name the application's event store.</param>
public class TenantEventStores(IChronicleClient client, IOptions<ChronicleClientOptions> options) : ITenantEventStores
{
    /// <summary>
    /// Opens the event store in the default namespace and every other namespace it has.
    /// </summary>
    /// <param name="cancellationToken">Stops waiting when the host shuts down.</param>
    /// <returns>The event store of each namespace, the default namespace first.</returns>
    public async Task<IReadOnlyList<IEventStore>> All(CancellationToken cancellationToken = default)
    {
        var name = options.Value.EventStore;
        var root = await client.GetEventStore(name, EventStoreNamespaceName.Default).WaitAsync(cancellationToken);
        var stores = new List<IEventStore> { root };
        foreach (var @namespace in await root.GetNamespaces(cancellationToken))
        {
            if (string.Equals(@namespace.Value, EventStoreNamespaceName.Default.Value, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            stores.Add(await client.GetEventStore(name, @namespace).WaitAsync(cancellationToken));
        }

        return stores;
    }

    /// <inheritdoc/>
    public async Task<IEventStore?> For(TenantId tenant, CancellationToken cancellationToken = default)
    {
        var name = options.Value.EventStore;
        var root = await client.GetEventStore(name, EventStoreNamespaceName.Default).WaitAsync(cancellationToken);
        var namespaces = await root.GetNamespaces(cancellationToken);
        var @namespace = namespaces.FirstOrDefault(_ => string.Equals(_.Value, tenant.Value, StringComparison.OrdinalIgnoreCase));
        return @namespace is null ? null : await client.GetEventStore(name, @namespace).WaitAsync(cancellationToken);
    }
}
