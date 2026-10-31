// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Prices;

/// <summary>
/// The price of a seat SKU as Direct's Admin catalog set it: the monthly price and the discount for paying yearly.
/// </summary>
/// <param name="Currency">The currency.</param>
/// <param name="MonthlyPrice">The price per month, in minor units.</param>
/// <param name="YearlyDiscount">The discount for paying yearly.</param>
public record SeatSkuPricing(CurrencyCode Currency, PriceInMinorUnits MonthlyPrice, YearlyDiscount YearlyDiscount);
