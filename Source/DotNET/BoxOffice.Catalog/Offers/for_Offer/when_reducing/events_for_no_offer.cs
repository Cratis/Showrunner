// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class events_for_no_offer : Specification
{
    Offer?[] _offers;

    void Because() => _offers =
    [
        Offer.Reduce(new SeatPlanSpecified(SeatCount.NotSet, false, false), null),
        Offer.Reduce(new ModuleOfferSpecified(ModuleId.NotSet, ModuleCharging.PerOrganization, TrialLength.NotSet, ModuleAvailability.Paid), null),
        Offer.Reduce(new ComputePackSpecified(MeterId.NotSet, MeterQuantity.NotSet, false), null),
        Offer.Reduce(new OfferPriced(CurrencyCode.NotSet, PriceInMinorUnits.NotSet, YearlyDiscount.NotSet), null),
        Offer.Reduce(new OfferIncludedModule("reporting"), null),
        Offer.Reduce(new OfferIncludedAllowance(MeterId.NotSet, MeterQuantity.NotSet, AllowancePeriod.Day), null),
        Offer.Reduce(new OfferRetired(), null)
    ];

    [Fact] void should_not_produce_an_offer_for_any_of_them() => _offers.All(offer => offer is null).ShouldBeTrue();
}
#endif
