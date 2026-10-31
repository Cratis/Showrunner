// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineModuleOffer;

/// <summary>
/// Defines a module offer: one module sold on its own, flat per organization or per seat.
/// </summary>
/// <param name="OfferId">The offer's identity.</param>
/// <param name="Name">Its name.</param>
/// <param name="Description">What it is for.</param>
/// <param name="Module">The module it sells.</param>
/// <param name="Charging">How it is charged.</param>
/// <param name="Trial">How many days it can be tried before it is paid for; zero for no trial.</param>
/// <param name="Availability">Whether it is paid for or only previewed.</param>
[Command]
[Authorize]
public record DefineModuleOffer(
    OfferId OfferId,
    OfferName Name,
    OfferDescription Description,
    ModuleId Module,
    ModuleCharging Charging,
    TrialLength Trial,
    ModuleAvailability Availability)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The offer defined, then given its module.</returns>
    public IEnumerable<object> Handle() =>
    [
        new OfferDefined(OfferKind.Module, Name, Description),
        new ModuleOfferSpecified(Module, Charging, Trial, Availability)
    ];
}

/// <summary>
/// Validates <see cref="DefineModuleOffer"/>.
/// </summary>
public class DefineModuleOfferValidator : CommandValidator<DefineModuleOffer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefineModuleOfferValidator"/> class.
    /// </summary>
    /// <param name="offer">The offer with the command's id, <see langword="null"/> when it is not defined yet.</param>
    public DefineModuleOfferValidator(Offer? offer)
    {
        RuleFor(c => (Guid)c.OfferId).MustNotBeDefined(offer);
        RuleFor(c => (string)c.Name).MustBeNamed();
        RuleFor(c => (string)c.Description).MustFit();
        RuleFor(c => (string)c.Module).NotEmpty().WithMessage("Choose the module the offer sells.");
        RuleFor(c => (int)c.Trial).GreaterThanOrEqualTo(0).WithMessage("A trial cannot have a negative length.");
        RuleFor(c => c.Charging).IsInEnum().WithMessage("Choose how the module is charged.");
        RuleFor(c => c.Availability).IsInEnum().WithMessage("Choose whether the module is paid for or previewed.");
    }
}
