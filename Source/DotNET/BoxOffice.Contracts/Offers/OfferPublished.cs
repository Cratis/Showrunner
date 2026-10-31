// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offers;

/// <summary>
/// Published by Direct when an offer is made available or changed, so a product can show its plan page without
/// calling Direct. Appended under the offer's id.
/// </summary>
/// <param name="Name">The name of the offer.</param>
/// <param name="Modules">The modules the offer includes.</param>
/// <param name="IncludedSeats">The seats the offer includes.</param>
/// <param name="Prices">What the offer costs.</param>
[EventType]
[Public]
public record OfferPublished(
    OfferName Name,
    IEnumerable<ModuleId> Modules,
    SeatCount IncludedSeats,
    IEnumerable<OfferPrice> Prices);
