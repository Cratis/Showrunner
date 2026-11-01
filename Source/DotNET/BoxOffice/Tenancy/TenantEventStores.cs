// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Tenancy;

/// <summary>
/// Opens the event store of every tenant namespace of the application.
/// </summary>
public class TenantEventStores
{
    readonly IChronicleClient _client;
    readonly BoxOfficeEventStore _eventStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEventStores"/> class, naming the event store from the Chronicle
    /// client options only.
    /// </summary>
    /// <param name="client">The <see cref="IChronicleClient"/>.</param>
    /// <param name="options">The <see cref="ChronicleClientOptions"/> naming the application's event store.</param>
    public TenantEventStores(IChronicleClient client, IOptions<ChronicleClientOptions> options)
        : this(client, options, BoxOfficeEventStore.ForClientOptions(options))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEventStores"/> class.
    /// </summary>
    /// <param name="client">The <see cref="IChronicleClient"/>.</param>
    /// <param name="options">The <see cref="ChronicleClientOptions"/>. The event store is named by <paramref name="eventStore"/>; the parameter keeps this constructor a superset of the earlier one, so dependency injection chooses it.</param>
    /// <param name="eventStore">The <see cref="BoxOfficeEventStore"/> naming the application's event store.</param>
    public TenantEventStores(IChronicleClient client, IOptions<ChronicleClientOptions> options, BoxOfficeEventStore eventStore)
    {
        _ = options;
        _client = client;
        _eventStore = eventStore;
    }

    /// <summary>
    /// Gets the event store of the default namespace followed by that of every other namespace.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The event stores, the default namespace first.</returns>
    public async Task<IReadOnlyList<IEventStore>> All(CancellationToken cancellationToken = default)
    {
        var name = _eventStore.Name;
        var root = await _client.GetEventStore(name, EventStoreNamespaceName.Default).WaitAsync(cancellationToken);
        var stores = new List<IEventStore> { root };
        foreach (var @namespace in await root.GetNamespaces(cancellationToken))
        {
            if (string.Equals(@namespace.Value, EventStoreNamespaceName.Default.Value, StringComparison.Ordinal))
            {
                continue;
            }

            stores.Add(await _client.GetEventStore(name, @namespace).WaitAsync(cancellationToken));
        }

        return stores;
    }
}
