// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.Tenancy;

/// <summary>
/// Opens the collecting event store in every namespace it has.
/// </summary>
/// <remarks>
/// Chronicle runs the host's reactors and reducers per namespace, and only in the namespaces the host has opened. A web
/// request opens the namespace of its tenant, but the collecting side has work to do in a tenant also when nobody from it
/// is signed in: recording the notes that arrived and publishing the answers. Chronicle also forwards an application's
/// events only into namespaces the collecting store has. Opening a namespace registers the collecting observers in it.
/// </remarks>
/// <param name="client">The Chronicle client.</param>
/// <param name="options">The Chronicle client options, which name the collecting event store.</param>
public class TenantEventStores(IChronicleClient client, IOptions<ChronicleClientOptions> options)
{
    /// <summary>
    /// Opens the collecting store in the default namespace and every other namespace it has.
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
            if (string.Equals(@namespace.Value, EventStoreNamespaceName.Default.Value, StringComparison.Ordinal))
            {
                continue;
            }

            stores.Add(await client.GetEventStore(name, @namespace).WaitAsync(cancellationToken));
        }

        return stores;
    }
}
