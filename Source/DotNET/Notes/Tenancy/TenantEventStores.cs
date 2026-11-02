// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Tenancy;

/// <summary>
/// Opens the application's event store in every tenant namespace it has.
/// </summary>
/// <remarks>
/// Chronicle runs the application's reactors and reducers per namespace, and only in the namespaces the application
/// has opened. A web request opens the namespace of its tenant, but Notes has work to do in a tenant also when nobody
/// from it is signed in: publishing what was recorded before a restart and receiving the answers. Opening a namespace
/// registers Notes' observers in it.
/// </remarks>
public class TenantEventStores
{
    readonly IChronicleClient _client;
    readonly NotesEventStore _eventStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEventStores"/> class, naming the event store from the Chronicle
    /// client options only.
    /// </summary>
    /// <param name="client">The Chronicle client.</param>
    /// <param name="options">The Chronicle client options, which name the event store.</param>
    public TenantEventStores(IChronicleClient client, IOptions<ChronicleClientOptions> options)
        : this(client, options, NotesEventStore.ForClientOptions(options))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantEventStores"/> class.
    /// </summary>
    /// <param name="client">The Chronicle client.</param>
    /// <param name="options">The Chronicle client options. The event store is named by <paramref name="eventStore"/>; the parameter keeps this constructor a superset of the earlier one, so dependency injection chooses it.</param>
    /// <param name="eventStore">The <see cref="NotesEventStore"/> naming the event store.</param>
    public TenantEventStores(IChronicleClient client, IOptions<ChronicleClientOptions> options, NotesEventStore eventStore)
    {
        _ = options;
        _client = client;
        _eventStore = eventStore;
    }

    /// <summary>
    /// Opens the event store in the default namespace and every other namespace it has.
    /// </summary>
    /// <param name="cancellationToken">Stops waiting when the host shuts down.</param>
    /// <returns>The event store of each namespace, the default namespace first.</returns>
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
