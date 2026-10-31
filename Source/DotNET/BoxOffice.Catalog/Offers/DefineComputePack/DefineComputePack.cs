// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineComputePack;

/// <summary>
/// Defines a compute pack: an extra quantity of a meter per period, optionally stackable.
/// </summary>
/// <param name="OfferId">The offer's identity.</param>
/// <param name="Name">Its name.</param>
/// <param name="Description">What it is for.</param>
/// <param name="Meter">The meter it extends.</param>
/// <param name="Quantity">How much the meter's allowance grows per period.</param>
/// <param name="Stackable">Whether an organization can hold more than one.</param>
[Command]
[Authorize]
public record DefineComputePack(
    OfferId OfferId,
    OfferName Name,
    OfferDescription Description,
    MeterId Meter,
    MeterQuantity Quantity,
    bool Stackable)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The offer defined, then given its meter.</returns>
    public IEnumerable<object> Handle() =>
    [
        new OfferDefined(OfferKind.ComputePack, Name, Description),
        new ComputePackSpecified(Meter, Quantity, Stackable)
    ];
}

/// <summary>
/// Validates <see cref="DefineComputePack"/>.
/// </summary>
public class DefineComputePackValidator : CommandValidator<DefineComputePack>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefineComputePackValidator"/> class.
    /// </summary>
    /// <param name="offer">The offer with the command's id, <see langword="null"/> when it is not defined yet.</param>
    public DefineComputePackValidator(Offer? offer)
    {
        RuleFor(c => (Guid)c.OfferId).MustNotBeDefined(offer);
        RuleFor(c => (string)c.Name).MustBeNamed();
        RuleFor(c => (string)c.Description).MustFit();
        RuleFor(c => (string)c.Meter).NotEmpty().WithMessage("Choose the meter the pack extends.");
        RuleFor(c => (long)c.Quantity).GreaterThan(0).WithMessage("A pack adds at least one unit.");
    }
}
