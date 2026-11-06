// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.Contracts.Tenants;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// Starts new tenants on the product's default offering: <see cref="IDefaultOffering"/>.
/// </summary>
/// <remarks>
/// The product records <see cref="TenantStartedOnDefaultOffering"/> in the tenant's own stream, which gives the tenant the
/// offer's seats and modules at once, and then tells the operator <c>TenantStartedOnOffer</c> in the outbox of the tenant's
/// namespace, under the organization, like every tenant fact the product tells the operator. Delivery is at least once, so
/// both appends carry an idempotency key made from the tenant: starting the same tenant again, also from another instance,
/// keeps the one fact of each.
/// </remarks>
/// <param name="offering">The product's offering.</param>
/// <param name="offers">The <see cref="IPublishedOffers"/>.</param>
/// <param name="client">The Chronicle client.</param>
/// <param name="eventStore">The <see cref="BoxOfficeEventStore"/> naming the product's event store.</param>
/// <param name="logger">The logger.</param>
/// <param name="options">The <see cref="OfferingOptions"/>, which name the audience of the operator the product reports to; <see langword="null" /> addresses nothing.</param>
public class DefaultOffering(
    ProductOffering offering,
    IPublishedOffers offers,
    IChronicleClient client,
    BoxOfficeEventStore eventStore,
    ILogger<DefaultOffering> logger,
    IOptions<OfferingOptions>? options = null) : IDefaultOffering
{
    /// <summary>
    /// Derives the idempotency key of the start of a tenant: the same for every attempt to start it.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(TenantId tenant) => new($"boxoffice-default-offering-{tenant.Value}");

    /// <summary>
    /// Derives the idempotency key of the fact that entitles a tenant: the same for every attempt to start it.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey EntitlementKeyFor(TenantId tenant) => new($"boxoffice-default-offering-entitled-{tenant.Value}");

    /// <inheritdoc/>
    public async Task<TenantStart> StartTenant(TenantId tenant, OrganizationId organization, CancellationToken cancellationToken = default)
    {
        if (offering.DefaultOffer is not { } name)
        {
            throw new DefaultOfferingNotDeclared();
        }

        var published = (await offers.All(cancellationToken)).Where(_ => string.Equals(_.Name.Value, name.Value, StringComparison.OrdinalIgnoreCase)).ToArray();
        var offer = published.FirstOrDefault(_ => !_.Withdrawn) ?? throw new DefaultOfferingNotPublished(name, published.Length > 0);

        var store = await client.GetEventStore(eventStore.Name, new EventStoreNamespaceName(tenant.Value)).WaitAsync(cancellationToken);

        // The tenant's own stream first: the tenant is entitled before the operator is told, so a failure in between is
        // retried by starting the tenant again.
        var entitled = await store.EventLog.AppendOnce(
            TenantStreamId.Value,
            new TenantStartedOnDefaultOffering(offer.Name, offer.IncludedSeats, [.. offer.Modules]),
            EntitlementKeyFor(tenant));
        if (!entitled.IsSuccess && !entitled.WasAlreadyAppended && !entitled.HasIdempotencyConflicts)
        {
            throw new TenantNotStarted(tenant, entitled);
        }

        var started = new TenantStartedOnOffer(tenant, offer.Name, offer.IncludedSeats, [.. offer.Modules]);
        using var audience = options?.Value.AddressedToOperator();
        var result = await store.GetEventSequence(EventSequenceId.Outbox).AppendOnce(organization, started, KeyFor(tenant));
        if (result.WasAlreadyAppended || result.HasIdempotencyConflicts)
        {
            // Another attempt started the tenant already; it may even have seen another version of the offer.
            logger.AlreadyStarted(tenant, name);
            return TenantStart.AlreadyStarted;
        }

        if (!result.IsSuccess)
        {
            throw new TenantNotStarted(tenant, result);
        }

        logger.TenantStarted(tenant, name);
        return TenantStart.Started;
    }
}
