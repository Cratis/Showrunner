// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_observing_offers;

public class all_offers : a_catalog_query
{
    void Establish()
    {
        Given(OfferId.New(), OfferEvents.Then(new OfferDefined(OfferKind.SeatPlan, "Team", "For teams."), new SeatPlanSpecified(10, false, false), OfferEvents.Priced));
        Given(OfferId.New(), OfferEvents.Then(new OfferDefined(OfferKind.Module, "Audit", "Audit trail."), new ModuleOfferSpecified("audit", ModuleCharging.PerOrganization, 0, ModuleAvailability.Paid), OfferEvents.Priced));
        Given(OfferId.New(), OfferEvents.Then(new OfferDefined(OfferKind.SeatPlan, "Legacy", "Gone."), new SeatPlanSpecified(5, false, false), OfferEvents.Priced, new OfferRetired()));
        Given(OfferId.New(), OfferEvents.Then(new OfferDefined(OfferKind.SeatPlan, "Draft", "Not priced yet."), new SeatPlanSpecified(3, false, false)));
    }

    async Task Because() => _result = await _scenario.Perform(nameof(Offer.AllOffers));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_every_offer_by_name() => ((IEnumerable<Offer>)_result.Data).Select(offer => (string)offer.Name).ShouldEqual(["Audit", "Draft", "Legacy", "Team"]);
}
#endif
