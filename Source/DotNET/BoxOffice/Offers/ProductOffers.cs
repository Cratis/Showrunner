// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering;
using Cratis.Showrunner.BoxOffice.Offering.Starting;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offers;

/// <summary>
/// Reads the offers, which concern the product as a whole and are kept where the operator published them: the default
/// namespace, not a tenant's.
/// </summary>
public class ProductOffers : IPublishedOffers
{
    readonly IChronicleClient _client;
    readonly BoxOfficeEventStore _eventStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductOffers"/> class, naming the event store from the Chronicle client
    /// options only.
    /// </summary>
    /// <param name="client">The <see cref="IChronicleClient"/>.</param>
    /// <param name="options">The <see cref="ChronicleClientOptions"/> naming the application's event store.</param>
    public ProductOffers(IChronicleClient client, IOptions<ChronicleClientOptions> options)
        : this(client, options, BoxOfficeEventStore.ForClientOptions(options))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductOffers"/> class.
    /// </summary>
    /// <param name="client">The <see cref="IChronicleClient"/>.</param>
    /// <param name="options">The <see cref="ChronicleClientOptions"/>. The event store is named by <paramref name="eventStore"/>; the parameter keeps this constructor a superset of the earlier one, so dependency injection chooses it.</param>
    /// <param name="eventStore">The <see cref="BoxOfficeEventStore"/> naming the application's event store.</param>
    public ProductOffers(IChronicleClient client, IOptions<ChronicleClientOptions> options, BoxOfficeEventStore eventStore)
    {
        _ = options;
        _client = client;
        _eventStore = eventStore;
    }

    /// <summary>
    /// Gets every offer, withdrawn or not.
    /// </summary>
    /// <returns>The offers.</returns>
    public async Task<IEnumerable<Offer>> All()
    {
        var store = await _client.GetEventStore(_eventStore.Name, EventStoreNamespaceName.Default);
        return await store.ReadModels.GetInstances<Offer>();
    }

    /// <inheritdoc/>
    async Task<IEnumerable<PublishedOffer>> IPublishedOffers.All(CancellationToken cancellationToken) =>
        [.. (await All()).Select(_ => new PublishedOffer(_.Id, _.Name, _.Modules, _.IncludedSeats, _.Withdrawn))];
}
