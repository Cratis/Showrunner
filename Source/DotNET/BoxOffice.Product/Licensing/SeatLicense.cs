// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Licensing;

/// <summary>
/// The answer to whether a person may take a seat in a tenant.
/// </summary>
/// <param name="Status">Whether the person may take a seat.</param>
/// <param name="WhoToAsk">Who the person can ask for a seat when there is none: the people who administer the tenant.</param>
public record SeatLicense(SeatLicenseStatus Status, IEnumerable<Person> WhoToAsk)
{
    /// <summary>
    /// The person may take a seat.
    /// </summary>
    public static readonly SeatLicense Licensed = new(SeatLicenseStatus.Licensed, []);

    /// <summary>
    /// The product has no such tenant.
    /// </summary>
    public static readonly SeatLicense UnknownOrganization = new(SeatLicenseStatus.UnknownOrganization, []);

    /// <summary>
    /// Gets a value indicating whether the person may take a seat.
    /// </summary>
    public bool MayTakeASeat => Status == SeatLicenseStatus.Licensed;

    /// <summary>
    /// The tenant has no seat for the person.
    /// </summary>
    /// <param name="whoToAsk">Who the person can ask for a seat.</param>
    /// <returns>The <see cref="SeatLicense"/>.</returns>
    public static SeatLicense NoLicense(IEnumerable<Person> whoToAsk) => new(SeatLicenseStatus.NoLicense, whoToAsk);
}
