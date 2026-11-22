// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// <see cref="IChronicleDirectory"/> over the Chronicle client, so the connection resolution of the Production applies as it does for an application.
/// </summary>
/// <param name="connectionString">The Chronicle connection string. It carries credentials and is never logged.</param>
public class ChronicleDirectory(string connectionString) : IChronicleDirectory
{
    /// <inheritdoc/>
    public async Task<ChronicleTopology> GetTopologyAsync(CancellationToken cancellationToken)
    {
        using var client = new ChronicleClient(connectionString);
        var eventStores = new List<EventStoreTopology>();
        foreach (var name in await client.GetEventStores(cancellationToken))
        {
            // The System event store may only be read by an administrator, which the applications' client is not. Its one namespace is known.
            if (string.Equals(name.Value, ChronicleDatabaseNames.SystemEventStore, StringComparison.OrdinalIgnoreCase))
            {
                eventStores.Add(new(name.Value, [ChronicleDatabaseNames.DefaultNamespace]));
                continue;
            }

            var eventStore = await client.GetEventStore(name);
            var namespaces = (await eventStore.GetNamespaces(cancellationToken)).Select(@namespace => @namespace.Value).ToList();
            eventStores.Add(new(name.Value, namespaces));
        }

        return new(eventStores);
    }
}
