// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Offers;

/// <summary>
/// An offer the operator sells the product under, kept for the plan page. A withdrawn offer stays as it was, marked
/// withdrawn: tenants that have it keep it.
/// </summary>
/// <param name="Id">The id of the offer.</param>
/// <param name="Name">The name of the offer.</param>
/// <param name="Modules">The modules the offer includes.</param>
/// <param name="IncludedSeats">The seats the offer includes.</param>
/// <param name="Prices">What the offer costs.</param>
/// <param name="Withdrawn">Whether the operator stopped selling it.</param>
[ReadModel]
public record Offer(OfferId Id, OfferName Name, IEnumerable<ModuleId> Modules, SeatCount IncludedSeats, IEnumerable<OfferPrice> Prices, bool Withdrawn)
{
    /// <summary>
    /// Gets the offers on sale.
    /// </summary>
    /// <param name="offers">The <see cref="ProductOffers"/>.</param>
    /// <returns>The offers the operator sells.</returns>
    [Authorize]
    public static async Task<IEnumerable<Offer>> OnSale(ProductOffers offers) => [.. (await offers.All()).Where(_ => !_.Withdrawn).OrderBy(_ => _.Name.Value)];

    /// <summary>
    /// Folds <see cref="OperatorPublishedOffer"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static Offer Reduce(OperatorPublishedOffer @event, Offer? current, EventContext context) =>
        new(new(Guid.Parse(context.EventSourceId.Value)), @event.Name, @event.Modules, @event.IncludedSeats, @event.Prices, false);

    /// <summary>
    /// Folds <see cref="OperatorWithdrewOffer"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static Offer? Reduce(OperatorWithdrewOffer @event, Offer? current) => current is null ? null : current with { Withdrawn = true };
}
