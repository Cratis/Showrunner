// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_prorating;

public class added_seats_before_the_period_started : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.Prorate(10000, 10, 2, new DateOnly(2026, 7, 1), BillingInterval.Monthly, new DateOnly(2026, 5, 1));

    [Fact] void should_charge_for_no_more_than_the_whole_period() => _amount.ShouldEqual(new PriceInMinorUnits(2000));
}
#endif
