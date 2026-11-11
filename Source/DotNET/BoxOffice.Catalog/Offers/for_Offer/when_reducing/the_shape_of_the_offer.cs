// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class the_shape_of_the_offer : Specification
{
    string[] _parameters;
    int _reducers;

    void Because()
    {
        _parameters = [.. typeof(Offer).GetConstructors().Single().GetParameters().Select(parameter => $"{parameter.Name}:{parameter.ParameterType.Name}")];
        _reducers = typeof(Offer).GetMethods(BindingFlags.Public | BindingFlags.Static).Count(method => method.Name == nameof(Offer.Reduce));
    }

    [Fact]
    void should_keep_its_members_and_their_order() => _parameters.ShouldEqual(
    [
        "Id:OfferId",
        "Kind:OfferKind",
        "Name:OfferName",
        "Description:OfferDescription",
        "Seats:SeatCount",
        "UnlimitedSeats:Boolean",
        "RequiresSalesContact:Boolean",
        "Module:ModuleId",
        "Charging:ModuleCharging",
        "Trial:TrialLength",
        "Availability:ModuleAvailability",
        "Meter:MeterId",
        "PackQuantity:MeterQuantity",
        "Stackable:Boolean",
        "Priced:Boolean",
        "Currency:CurrencyCode",
        "MonthlyPrice:PriceInMinorUnits",
        "YearlyDiscount:YearlyDiscount",
        "IncludedModules:IEnumerable`1",
        "IncludedAllowances:IEnumerable`1",
        "Retired:Boolean"
    ]);

    [Fact] void should_keep_one_reducer_for_each_event() => _reducers.ShouldEqual(8);
}
#endif
