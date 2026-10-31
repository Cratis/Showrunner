// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offering;

/// <summary>
/// Whether a product licenses its use per person.
/// </summary>
public enum SeatLicensing
{
    /// <summary>
    /// Anyone in a tenant may use the product.
    /// </summary>
    NotLicensed = 0,

    /// <summary>
    /// Each person needs a seat.
    /// </summary>
    PerUser = 1
}
