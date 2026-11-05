// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Product.Provisioning;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Product.Licensing;

/// <summary>
/// Answers whether a person may take a seat from the seats the operator decided on and who holds them.
/// </summary>
/// <param name="stores">The <see cref="ITenantStores"/> that finds the tenant.</param>
/// <param name="time">The <see cref="TimeProvider"/> the days are counted by.</param>
public class SeatLicenses(ITenantStores stores, TimeProvider time) : ISeatLicenses
{
    /// <inheritdoc/>
    public async Task<SeatLicense> For(TenantId tenant, SeatHolder holder, CancellationToken cancellationToken = default)
    {
        if (await stores.For(tenant, cancellationToken) is not { } store)
        {
            return SeatLicense.UnknownOrganization;
        }

        var entitlements = new TenantEntitlements(store.ReadModels, time);
        if (await entitlements.WhyReadOnly() != ReadOnlyReason.None)
        {
            return await NoLicense(store);
        }

        var capacity = await entitlements.SeatCapacity();
        var roster = await store.ReadModels.GetInstanceById<SeatRoster>(TenantStream.Id.Value);
        var held = roster?.Held ?? 0;
        return roster?.IsLicensed(holder, capacity) == true || (roster?.Holds(holder) != true && held < capacity)
            ? SeatLicense.Licensed
            : await NoLicense(store);
    }

    static async Task<SeatLicense> NoLicense(Cratis.Chronicle.IEventStore store)
    {
        var creator = await store.ReadModels.GetInstanceById<TenantCreator>(TenantStream.Id.Value);
        return SeatLicense.NoLicense(creator is null ? [] : [creator.Person]);
    }
}
