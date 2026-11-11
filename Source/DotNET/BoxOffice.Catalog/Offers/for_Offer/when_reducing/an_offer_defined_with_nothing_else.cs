// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class an_offer_defined_with_nothing_else : Specification
{
    OfferId _id;
    Offer _offer;

    void Establish() => _id = OfferId.New();

    void Because() => _offer = Offer.Reduce(
        new OfferDefined(OfferKind.Module, "Reporting", "Shows the numbers."),
        null,
        EventContext.From(
            EventStoreName.NotSet,
            EventStoreNamespaceName.NotSet,
            EventType.Unknown,
            EventSourceType.Default,
            _id,
            EventStreamType.All,
            EventStreamId.Default,
            0,
            CorrelationId.New(),
            DateTimeOffset.UnixEpoch));

    [Fact] void should_be_the_offer_of_the_event_source() => _offer.Id.ShouldEqual(_id);
    [Fact] void should_take_what_it_sells_its_name_and_its_description_from_the_event() => (_offer.Kind, _offer.Name, _offer.Description).ShouldEqual((OfferKind.Module, new OfferName("Reporting"), new OfferDescription("Shows the numbers.")));
    [Fact] void should_have_no_seats() => (_offer.Seats, _offer.UnlimitedSeats, _offer.RequiresSalesContact).ShouldEqual((SeatCount.NotSet, false, false));
    [Fact] void should_have_no_module_yet_charged_per_organization_and_paid() => (_offer.Module, _offer.Charging, _offer.Trial, _offer.Availability).ShouldEqual((ModuleId.NotSet, ModuleCharging.PerOrganization, TrialLength.NotSet, ModuleAvailability.Paid));
    [Fact] void should_have_no_compute_pack() => (_offer.Meter, _offer.PackQuantity, _offer.Stackable).ShouldEqual((MeterId.NotSet, MeterQuantity.NotSet, false));
    [Fact] void should_not_be_priced() => (_offer.Priced, _offer.Currency, _offer.MonthlyPrice, _offer.YearlyDiscount).ShouldEqual((false, CurrencyCode.NotSet, PriceInMinorUnits.NotSet, YearlyDiscount.NotSet));
    [Fact] void should_include_nothing() => (_offer.IncludedModules.Any() || _offer.IncludedAllowances.Any()).ShouldBeFalse();
    [Fact] void should_not_be_retired() => _offer.Retired.ShouldBeFalse();
}
#endif
