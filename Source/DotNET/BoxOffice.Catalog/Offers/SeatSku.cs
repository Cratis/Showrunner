// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// A seat SKU as Direct's Admin catalog defined it before offers existed: a seat package with a name, how many seats it
/// carries and whether it is bought through sales.
/// </summary>
/// <param name="Name">Its name.</param>
/// <param name="Description">What it is for.</param>
/// <param name="Seats">How many seats it carries; not used when it is unlimited.</param>
/// <param name="Unlimited">Whether it carries unlimited seats.</param>
/// <param name="RequiresSalesContact">Whether it is bought through sales rather than checkout.</param>
public record SeatSku(OfferName Name, OfferDescription Description, SeatCount Seats, bool Unlimited, bool RequiresSalesContact);
