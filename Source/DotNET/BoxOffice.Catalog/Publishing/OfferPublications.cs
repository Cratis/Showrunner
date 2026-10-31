// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing;

/// <summary>
/// Decides what an offer's public face is: the <see cref="OfferPublished"/> a product reads to show its plan page, or
/// nothing while the offer is not on sale.
/// </summary>
public static class OfferPublications
{
    /// <summary>
    /// What <see cref="OfferPublished.IncludedSeats"/> holds for a seat plan with unlimited seats, since the public
    /// contract has no separate flag.
    /// </summary>
    public static readonly SeatCount UnlimitedSeats = new(int.MaxValue);

    /// <summary>
    /// Gets the public face of an offer.
    /// </summary>
    /// <param name="offer">The offer, <see langword="null"/> when it is not defined.</param>
    /// <returns>What to publish; <see langword="null"/> when the offer is not defined, not priced, or retired.</returns>
    public static OfferPublished? For(Offer? offer) => offer is { Priced: true, Retired: false }
        ? new OfferPublished(
            offer.Name,
            offer.Kind == OfferKind.Module ? [offer.Module, .. offer.IncludedModules] : offer.IncludedModules,
            SeatsOf(offer),
            [
                new OfferPrice(BillingInterval.Monthly, offer.TotalFor(BillingInterval.Monthly), offer.Currency),
                new OfferPrice(BillingInterval.Yearly, offer.TotalFor(BillingInterval.Yearly), offer.Currency)
            ])
        : null;

    /// <summary>
    /// Gets whether two publications say the same thing.
    /// </summary>
    /// <param name="left">One publication.</param>
    /// <param name="right">The other.</param>
    /// <returns>True when publishing the second after the first would tell a product nothing new.</returns>
    public static bool Same(OfferPublished left, OfferPublished right) =>
        left.Name == right.Name &&
        left.IncludedSeats == right.IncludedSeats &&
        left.Modules.SequenceEqual(right.Modules) &&
        left.Prices.SequenceEqual(right.Prices);

    static SeatCount SeatsOf(Offer offer)
    {
        if (offer.Kind != OfferKind.SeatPlan)
        {
            return SeatCount.NotSet;
        }

        return offer.UnlimitedSeats ? UnlimitedSeats : offer.Seats;
    }
}
