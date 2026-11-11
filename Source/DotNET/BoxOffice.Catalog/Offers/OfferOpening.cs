// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// Opens an <see cref="Offer"/>. It lives outside the read model because Arc treats every static method on a read model as a query.
/// </summary>
internal static class OfferOpening
{
    /// <summary>
    /// Opens an offer with what its definition says and every other detail not set.
    /// </summary>
    /// <param name="id">The offer.</param>
    /// <param name="event">The definition.</param>
    /// <returns>The offer, not yet priced, with nothing included.</returns>
    internal static Offer From(OfferId id, OfferDefined @event) => new(
        id,
        @event.Kind,
        @event.Name,
        @event.Description,
        SeatCount.NotSet,
        false,
        false,
        ModuleId.NotSet,
        ModuleCharging.PerOrganization,
        TrialLength.NotSet,
        ModuleAvailability.Paid,
        MeterId.NotSet,
        MeterQuantity.NotSet,
        false,
        false,
        CurrencyCode.NotSet,
        PriceInMinorUnits.NotSet,
        YearlyDiscount.NotSet,
        [],
        [],
        false);
}
