// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.PriceOffer;

/// <summary>
/// Sets an offer's price: the monthly price and the discount for paying yearly.
/// </summary>
/// <param name="OfferId">The offer.</param>
/// <param name="Currency">The currency, as a three-letter ISO 4217 code.</param>
/// <param name="MonthlyPrice">The price per month, in minor units.</param>
/// <param name="YearlyDiscount">The discount for paying yearly, between 0 and 100 percent.</param>
[Command]
[Authorize]
public record PriceOffer(OfferId OfferId, CurrencyCode Currency, PriceInMinorUnits MonthlyPrice, YearlyDiscount YearlyDiscount)
{
    /// <summary>
    /// Handles the command, recording nothing when the offer already has this price.
    /// </summary>
    /// <param name="offer">The offer, read under the command's consistency boundary.</param>
    /// <returns>The price, or nothing when it stands.</returns>
    public IEnumerable<object> Handle(Offer? offer) =>
        offer is { Priced: true } && offer.Currency == Currency && offer.MonthlyPrice == MonthlyPrice && offer.YearlyDiscount == YearlyDiscount
            ? []
            : [new OfferPriced(Currency, MonthlyPrice, YearlyDiscount)];
}

/// <summary>
/// Validates <see cref="PriceOffer"/>.
/// </summary>
public class PriceOfferValidator : CommandValidator<PriceOffer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PriceOfferValidator"/> class.
    /// </summary>
    /// <param name="offer">The offer, <see langword="null"/> when it is not defined.</param>
    public PriceOfferValidator(Offer? offer)
    {
        RuleFor(c => (Guid)c.OfferId).MustBeOnSale(offer);
        RuleFor(c => (string)c.Currency).Length(3).WithMessage("Give the currency as a three-letter code.");
        RuleFor(c => (long)c.MonthlyPrice).GreaterThanOrEqualTo(0).WithMessage("A price cannot be negative.");
        RuleFor(c => (decimal)c.YearlyDiscount).InclusiveBetween(0m, 100m).WithMessage("The yearly discount is a percentage between 0 and 100.");
    }
}
