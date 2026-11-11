// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// Folds what an <see cref="Offer"/> includes into it. It lives outside the read model because Arc treats every static method on a read model as a query.
/// </summary>
internal static class OfferInclusions
{
    /// <summary>
    /// Adds a module to what an offer includes, unless it already includes it.
    /// </summary>
    /// <param name="offer">The offer.</param>
    /// <param name="module">The module.</param>
    /// <returns>The offer including the module once.</returns>
    internal static Offer IncludingModule(Offer offer, ModuleId module) =>
        offer.IncludedModules.Contains(module) ? offer : offer with { IncludedModules = [.. offer.IncludedModules, module] };

    /// <summary>
    /// Adds an allowance to what an offer includes, replacing the one for the same meter and period.
    /// </summary>
    /// <param name="offer">The offer.</param>
    /// <param name="event">The allowance.</param>
    /// <returns>The offer with the allowance included.</returns>
    internal static Offer IncludingAllowance(Offer offer, OfferIncludedAllowance @event) => offer with
    {
        IncludedAllowances =
        [
            .. offer.IncludedAllowances.Where(allowance => allowance.Meter != @event.Meter || allowance.Period != @event.Period),
            new OfferAllowance(@event.Meter, @event.Quantity, @event.Period)
        ]
    };
}
