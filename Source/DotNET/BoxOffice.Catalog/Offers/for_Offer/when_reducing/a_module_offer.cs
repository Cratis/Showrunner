// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class a_module_offer : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Reporting);
        _offer = _scenario.Instance;
    }

    [Fact] void should_be_a_module_offer() => _offer.Kind.ShouldEqual(OfferKind.Module);
    [Fact] void should_sell_the_module() => _offer.Module.ShouldEqual(new ModuleId("reporting"));
    [Fact] void should_charge_per_seat() => _offer.Charging.ShouldEqual(ModuleCharging.PerSeat);
    [Fact] void should_have_the_trial() => _offer.Trial.ShouldEqual(new TrialLength(14));
    [Fact] void should_be_paid() => _offer.Availability.ShouldEqual(ModuleAvailability.Paid);
}
#endif
