// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Licensing;

/// <summary>
/// Whether a person may take a seat in a tenant.
/// </summary>
public enum SeatLicenseStatus
{
    /// <summary>
    /// The person holds a seat the tenant's seats cover, or a seat is free for them to take.
    /// </summary>
    Licensed = 0,

    /// <summary>
    /// The tenant has no seat for the person: its seats are all held, it has none, or it is read-only.
    /// </summary>
    NoLicense = 1,

    /// <summary>
    /// The product has no such tenant.
    /// </summary>
    UnknownOrganization = 2
}
