// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.BoxOffice.Entitlements;

namespace Cratis.Showrunner.BoxOffice.Seats;

/// <summary>
/// Whether a user holds a seat that is licensed.
/// </summary>
public enum SeatStatus
{
    /// <summary>
    /// The user holds no seat.
    /// </summary>
    NoSeat = 0,

    /// <summary>
    /// The user holds a seat the tenant's seats cover.
    /// </summary>
    Licensed = 1,

    /// <summary>
    /// The user holds a seat, but the tenant has fewer seats than are held and the seats of those who were assigned
    /// first are the ones that count.
    /// </summary>
    OverCapacity = 2
}

/// <summary>
/// Where the signed-in user stands with the tenant's seats, and why when they hold none they may use.
/// </summary>
/// <param name="Status">Whether the user holds a seat that is licensed.</param>
/// <param name="Capacity">How many seats the tenant may use.</param>
/// <param name="Held">How many seats are held.</param>
/// <param name="ReadOnlyReason">Why the tenant can not change anything, when it can not.</param>
[ReadModel]
public record SeatStanding(SeatStatus Status, int Capacity, int Held, ReadOnlyReason ReadOnlyReason)
{
    /// <summary>
    /// Gets where the signed-in user stands.
    /// </summary>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current tenant.</param>
    /// <param name="entitlements">The <see cref="IEntitlements"/> of the current tenant.</param>
    /// <param name="holder">The <see cref="ICurrentSeatHolder"/>.</param>
    /// <returns>The standing.</returns>
    [Authorize]
    public static async Task<SeatStanding> Mine(IReadModels readModels, IEntitlements entitlements, ICurrentSeatHolder holder)
    {
        var roster = await readModels.GetInstanceById<SeatRoster>(TenantStream.Id.Value);
        var capacity = await entitlements.SeatCapacity();
        var held = roster?.Held ?? 0;
        return new(StatusOf(holder.Current, roster, capacity), capacity, held, await entitlements.WhyReadOnly());
    }

    static SeatStatus StatusOf(SeatHolder? me, SeatRoster? roster, int capacity)
    {
        if (me is null || roster?.Holds(me) != true)
        {
            return SeatStatus.NoSeat;
        }

        return roster.IsLicensed(me, capacity) ? SeatStatus.Licensed : SeatStatus.OverCapacity;
    }
}
