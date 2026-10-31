// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Events;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing;

/// <summary>
/// Folds an offer's own events into the offer, with the same reducers the catalog's read model uses. It reads the
/// events themselves, which cannot be behind, never a read model, which can.
/// </summary>
public static class OfferHistory
{
    /// <summary>
    /// The event types that change what is published about an offer.
    /// </summary>
    public static readonly IEnumerable<Type> PublicationEvents =
    [
        typeof(OfferDefined),
        typeof(SeatPlanSpecified),
        typeof(ModuleOfferSpecified),
        typeof(OfferPriced),
        typeof(OfferIncludedModule),
        typeof(OfferRetired)
    ];

    /// <summary>
    /// Folds events, in the order they were appended, into the offer they build.
    /// </summary>
    /// <param name="events">The offer's events.</param>
    /// <returns>The offer; <see langword="null"/> when none of the events defines it.</returns>
    public static Offer? Fold(IEnumerable<AppendedEvent> events)
    {
        Offer? offer = null;
        foreach (var appended in events)
        {
            offer = appended.Content switch
            {
                OfferDefined @event => Offer.Reduce(@event, offer, appended.Context),
                SeatPlanSpecified @event => Offer.Reduce(@event, offer),
                ModuleOfferSpecified @event => Offer.Reduce(@event, offer),
                ComputePackSpecified @event => Offer.Reduce(@event, offer),
                OfferPriced @event => Offer.Reduce(@event, offer),
                OfferIncludedModule @event => Offer.Reduce(@event, offer),
                OfferIncludedAllowance @event => Offer.Reduce(@event, offer),
                OfferRetired @event => Offer.Reduce(@event, offer),
                _ => offer
            };
        }

        return offer;
    }
}
