// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// The kind of thing an offer sells.
/// </summary>
public enum OfferKind
{
    /// <summary>
    /// A plan with a number of seats, or unlimited seats, and what it includes.
    /// </summary>
    SeatPlan = 0,

    /// <summary>
    /// One module, sold on its own.
    /// </summary>
    Module = 1,

    /// <summary>
    /// An extra quantity of a compute meter.
    /// </summary>
    ComputePack = 2,
}
