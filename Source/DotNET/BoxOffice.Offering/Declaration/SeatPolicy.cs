// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// How the product licenses seats.
/// </summary>
/// <param name="Licensing">Whether people need a seat.</param>
/// <param name="Roles">The tenant roles that need a seat; empty means every role does. Meaningless when seats are not licensed.</param>
public record SeatPolicy(SeatLicensing Licensing, IEnumerable<TenantRole> Roles)
{
    /// <summary>
    /// Gets the policy of a product that does not license seats.
    /// </summary>
    public static readonly SeatPolicy NotLicensed = new(SeatLicensing.NotLicensed, []);

    /// <summary>
    /// Gets whether a person with a role needs a seat to use the product.
    /// </summary>
    /// <param name="role">The person's role in the tenant.</param>
    /// <returns><see langword="true"/> when a seat is needed.</returns>
    public bool RequiresSeat(TenantRole role) =>
        Licensing == SeatLicensing.PerUser && (!Roles.Any() || Roles.Contains(role));
}
