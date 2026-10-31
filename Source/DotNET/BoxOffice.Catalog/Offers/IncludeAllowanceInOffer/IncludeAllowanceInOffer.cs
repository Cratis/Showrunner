// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.IncludeAllowanceInOffer;

/// <summary>
/// Makes a seat plan include a compute allowance, replacing the one it has for the same meter and period.
/// </summary>
/// <param name="OfferId">The seat plan.</param>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">How much of the meter per period.</param>
/// <param name="Period">How often the allowance renews.</param>
[Command]
[Authorize]
public record IncludeAllowanceInOffer(OfferId OfferId, MeterId Meter, MeterQuantity Quantity, AllowancePeriod Period)
{
    /// <summary>
    /// Handles the command, recording nothing when the seat plan already includes exactly this allowance.
    /// </summary>
    /// <param name="offer">The offer, read under the command's consistency boundary.</param>
    /// <returns>The inclusion, or nothing when it stands.</returns>
    public IEnumerable<object> Handle(Offer? offer) =>
        offer?.IncludedAllowances.Contains(new OfferAllowance(Meter, Quantity, Period)) == true ? [] : [new OfferIncludedAllowance(Meter, Quantity, Period)];
}

/// <summary>
/// Validates <see cref="IncludeAllowanceInOffer"/>.
/// </summary>
public class IncludeAllowanceInOfferValidator : CommandValidator<IncludeAllowanceInOffer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncludeAllowanceInOfferValidator"/> class.
    /// </summary>
    /// <param name="offer">The offer, <see langword="null"/> when it is not defined.</param>
    public IncludeAllowanceInOfferValidator(Offer? offer)
    {
        RuleFor(c => (Guid)c.OfferId).MustBeASeatPlanOnSale(offer);
        RuleFor(c => (string)c.Meter).NotEmpty().WithMessage("Choose the meter the allowance is for.");
        RuleFor(c => (long)c.Quantity).GreaterThan(0).WithMessage("An allowance is at least one unit.");
        RuleFor(c => c.Period).IsInEnum().WithMessage("Choose how often the allowance renews.");
    }
}
