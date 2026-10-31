// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// Keeps the meaning of an existing seat SKU: it becomes a seat plan offer that carries the same seats, the same sales
/// route and the same price, so adopting offers changes nothing a customer buys.
/// </summary>
public static class SeatPlanOffers
{
    /// <summary>
    /// Gets the events that define a seat SKU that has no price yet as a seat plan offer.
    /// </summary>
    /// <param name="sku">The seat SKU.</param>
    /// <returns>The events to append under the offer's id, in order.</returns>
    public static IEnumerable<object> From(SeatSku sku) =>
    [
        new OfferDefined(OfferKind.SeatPlan, sku.Name, sku.Description),
        new SeatPlanSpecified(sku.Seats, sku.Unlimited, sku.RequiresSalesContact)
    ];

    /// <summary>
    /// Gets the events that define a priced seat SKU as a seat plan offer.
    /// </summary>
    /// <param name="sku">The seat SKU.</param>
    /// <param name="pricing">Its price.</param>
    /// <returns>The events to append under the offer's id, in order.</returns>
    public static IEnumerable<object> From(SeatSku sku, SeatSkuPricing pricing) =>
    [
        .. From(sku),
        new OfferPriced(pricing.Currency, pricing.MonthlyPrice, pricing.YearlyDiscount)
    ];
}
