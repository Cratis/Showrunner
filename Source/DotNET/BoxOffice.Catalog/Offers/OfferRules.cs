// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// The rules the commands that change an offer share, so each is worded and enforced once.
/// </summary>
static class OfferRules
{
    /// <summary>
    /// Requires the offer to be defined, and not retired.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's offer id.</param>
    /// <param name="offer">The offer, or <see langword="null"/> when it is not defined.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, Guid> MustBeOnSale<T>(this IRuleBuilder<T, Guid> rule, Offer? offer) =>
        rule.Must(_ => offer is not null).WithMessage("This offer is not defined.")
            .Must(_ => offer?.Retired != true).WithMessage("This offer is retired.");

    /// <summary>
    /// Requires the offer to be defined, not retired, and a seat plan.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's offer id.</param>
    /// <param name="offer">The offer, or <see langword="null"/> when it is not defined.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, Guid> MustBeASeatPlanOnSale<T>(this IRuleBuilder<T, Guid> rule, Offer? offer) =>
        rule.MustBeOnSale(offer)
            .Must(_ => offer?.Kind is null or OfferKind.SeatPlan).WithMessage("Only a seat plan includes modules and allowances.");

    /// <summary>
    /// Requires the offer not to be defined yet.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's offer id.</param>
    /// <param name="offer">The offer, or <see langword="null"/> when it is not defined.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, Guid> MustNotBeDefined<T>(this IRuleBuilder<T, Guid> rule, Offer? offer) =>
        rule.Must(_ => offer is null).WithMessage("An offer with this id is already defined.");

    /// <summary>
    /// Requires an offer to have a name.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's name.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, string> MustBeNamed<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Name the offer.").MaximumLength(OfferLimits.NameLength).WithMessage($"Keep the name within {OfferLimits.NameLength} characters.");

    /// <summary>
    /// Requires an offer's description to fit.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="rule">The rule builder for the command's description.</param>
    /// <returns>The rule builder.</returns>
    internal static IRuleBuilderOptions<T, string> MustFit<T>(this IRuleBuilder<T, string> rule) =>
        rule.MaximumLength(OfferLimits.DescriptionLength).WithMessage($"Keep the description within {OfferLimits.DescriptionLength} characters.");
}
