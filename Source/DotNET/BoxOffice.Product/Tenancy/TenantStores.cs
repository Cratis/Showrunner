// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering;
using Cratis.Showrunner.BoxOffice.Tenancy;

namespace Cratis.Showrunner.BoxOffice.Product.Tenancy;

/// <summary>
/// Finds the event store of a tenant among the namespaces of the product's event store.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/> to find the stores with.</param>
/// <param name="eventStore">The <see cref="BoxOfficeEventStore"/> that names the product's event store.</param>
public class TenantStores(IChronicleClient client, BoxOfficeEventStore eventStore) : ITenantStores
{
    /// <inheritdoc/>
    public async Task<IEventStore?> For(TenantId tenant, CancellationToken cancellationToken = default)
    {
        var root = await client.GetEventStore(eventStore.Name, EventStoreNamespaceName.Default).WaitAsync(cancellationToken);
        var found = TenantNamespaces.Find(await root.GetNamespaces(cancellationToken), tenant);
        return found.Namespace is { } @namespace
            ? await client.GetEventStore(eventStore.Name, @namespace).WaitAsync(cancellationToken)
            : null;
    }
}
