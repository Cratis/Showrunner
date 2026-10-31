// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Tenancy;

/// <summary>
/// Opens the event store of every tenant namespace of the application.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="options">The <see cref="ChronicleClientOptions"/> naming the application's event store.</param>
public class TenantEventStores(IChronicleClient client, IOptions<ChronicleClientOptions> options)
{
    /// <summary>
    /// Gets the event store of the default namespace followed by that of every other namespace.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The event stores, the default namespace first.</returns>
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
