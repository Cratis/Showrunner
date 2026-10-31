// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// How a module offer is charged.
/// </summary>
public enum ModuleCharging
{
    /// <summary>
    /// One flat price for the organization, however many seats it has.
    /// </summary>
    PerOrganization = 0,

    /// <summary>
    /// The price is multiplied by the seats the organization has.
    /// </summary>
    PerSeat = 1,
}
