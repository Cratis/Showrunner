// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.IncludeModuleInOffer;

/// <summary>
/// Makes a seat plan include a module.
/// </summary>
/// <param name="OfferId">The seat plan.</param>
/// <param name="Module">The module.</param>
[Command]
[Authorize]
public record IncludeModuleInOffer(OfferId OfferId, ModuleId Module)
{
    /// <summary>
    /// Handles the command, recording nothing when the seat plan already includes the module.
    /// </summary>
    /// <param name="offer">The offer, read under the command's consistency boundary.</param>
    /// <returns>The inclusion, or nothing when it stands.</returns>
    public IEnumerable<object> Handle(Offer? offer) =>
        offer?.IncludedModules.Contains(Module) == true ? [] : [new OfferIncludedModule(Module)];
}

/// <summary>
/// Validates <see cref="IncludeModuleInOffer"/>.
/// </summary>
public class IncludeModuleInOfferValidator : CommandValidator<IncludeModuleInOffer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncludeModuleInOfferValidator"/> class.
    /// </summary>
    /// <param name="offer">The offer, <see langword="null"/> when it is not defined.</param>
    public IncludeModuleInOfferValidator(Offer? offer)
    {
        RuleFor(c => (Guid)c.OfferId).MustBeASeatPlanOnSale(offer);
        RuleFor(c => (string)c.Module).NotEmpty().WithMessage("Choose the module to include.");
    }
}
