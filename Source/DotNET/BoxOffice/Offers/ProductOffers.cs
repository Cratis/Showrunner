// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offers;

/// <summary>
/// Reads the offers, which concern the product as a whole and are kept where the operator published them: the default
/// namespace, not a tenant's.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="options">The <see cref="ChronicleClientOptions"/> naming the application's event store.</param>
public class ProductOffers(IChronicleClient client, IOptions<ChronicleClientOptions> options)
{
    /// <summary>
    /// Gets every offer, withdrawn or not.
    /// </summary>
    /// <returns>The offers.</returns>
    public async Task<IEnumerable<Offer>> All()
    {
        var store = await client.GetEventStore(options.Value.EventStore, EventStoreNamespaceName.Default);
        return await store.ReadModels.GetInstances<Offer>();
    }
}
