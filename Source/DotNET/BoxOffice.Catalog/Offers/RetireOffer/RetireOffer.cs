// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.RetireOffer;

/// <summary>
/// Retires an offer: it is no longer sold, and tenants that have it keep it.
/// </summary>
/// <param name="OfferId">The offer.</param>
[Command]
[Authorize]
public record RetireOffer(OfferId OfferId)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="OfferRetired"/> event.</returns>
    public OfferRetired Handle() => new();
}

/// <summary>
/// Validates <see cref="RetireOffer"/>.
/// </summary>
public class RetireOfferValidator : CommandValidator<RetireOffer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetireOfferValidator"/> class.
    /// </summary>
    /// <param name="offer">The offer, <see langword="null"/> when it is not defined.</param>
    public RetireOfferValidator(Offer? offer) => RuleFor(c => (Guid)c.OfferId).MustBeOnSale(offer);
}
