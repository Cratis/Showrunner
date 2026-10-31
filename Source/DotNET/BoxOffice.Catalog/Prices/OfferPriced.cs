// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Prices;

/// <summary>
/// An offer's price was set: the monthly price and the discount for paying yearly.
/// </summary>
/// <param name="Currency">The currency.</param>
/// <param name="MonthlyPrice">The price per month.</param>
/// <param name="YearlyDiscount">The discount for paying yearly.</param>
[EventType]
public record OfferPriced(CurrencyCode Currency, PriceInMinorUnits MonthlyPrice, YearlyDiscount YearlyDiscount);
