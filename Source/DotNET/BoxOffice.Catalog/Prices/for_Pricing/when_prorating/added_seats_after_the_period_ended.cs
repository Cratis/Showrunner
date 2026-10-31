// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_prorating;

public class added_seats_after_the_period_ended : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.Prorate(10000, 10, 2, new DateOnly(2026, 7, 1), BillingInterval.Monthly, new DateOnly(2026, 7, 20));

    [Fact] void should_charge_nothing() => _amount.ShouldEqual(new PriceInMinorUnits(0));
}
#endif
