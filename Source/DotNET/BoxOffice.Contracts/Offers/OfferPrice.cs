// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offers;

/// <summary>
/// What an offer costs for one billing interval.
/// </summary>
/// <param name="Interval">How often it is charged.</param>
/// <param name="Amount">The price.</param>
/// <param name="Currency">The currency.</param>
public record OfferPrice(
    BillingInterval Interval,
    PriceInMinorUnits Amount,
    CurrencyCode Currency);
