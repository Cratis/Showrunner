// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_prorating;

public class added_seats_halfway_through_a_year : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.Prorate(120000, 10, 1, new DateOnly(2027, 1, 1), BillingInterval.Yearly, new DateOnly(2026, 7, 2));

    [Fact] void should_charge_for_the_days_left_of_the_year() => _amount.ShouldEqual(new PriceInMinorUnits(6016));
}
#endif
