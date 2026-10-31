// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineSeatPlan;

/// <summary>
/// Defines a seat plan: an offer with seats, or unlimited seats, that can include modules and allowances.
/// </summary>
/// <param name="OfferId">The offer's identity.</param>
/// <param name="Name">Its name.</param>
/// <param name="Description">What it is for.</param>
/// <param name="Seats">How many seats it carries; not used when it is unlimited.</param>
/// <param name="Unlimited">Whether it carries unlimited seats.</param>
/// <param name="RequiresSalesContact">Whether it is bought through sales rather than checkout.</param>
[Command]
[Authorize]
public record DefineSeatPlan(
    OfferId OfferId,
    OfferName Name,
    OfferDescription Description,
    SeatCount Seats,
    bool Unlimited,
    bool RequiresSalesContact)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The offer defined, then given its seats.</returns>
    public IEnumerable<object> Handle() =>
    [
        new OfferDefined(OfferKind.SeatPlan, Name, Description),
        new SeatPlanSpecified(Seats, Unlimited, RequiresSalesContact)
    ];
}

/// <summary>
/// Validates <see cref="DefineSeatPlan"/>.
/// </summary>
public class DefineSeatPlanValidator : CommandValidator<DefineSeatPlan>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefineSeatPlanValidator"/> class.
    /// </summary>
    /// <param name="offer">The offer with the command's id, <see langword="null"/> when it is not defined yet.</param>
    public DefineSeatPlanValidator(Offer? offer)
    {
        RuleFor(c => (Guid)c.OfferId).MustNotBeDefined(offer);
        RuleFor(c => (string)c.Name).MustBeNamed();
        RuleFor(c => (string)c.Description).MustFit();
        RuleFor(c => (int)c.Seats)
            .GreaterThan(0).When(c => !c.Unlimited)
            .WithMessage("A seat plan that is not unlimited needs at least one seat.");
    }
}
