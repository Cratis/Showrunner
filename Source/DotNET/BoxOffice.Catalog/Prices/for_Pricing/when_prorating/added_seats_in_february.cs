// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_prorating;

public class added_seats_in_february : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.Prorate(2800, 4, 1, new DateOnly(2027, 3, 1), BillingInterval.Monthly, new DateOnly(2027, 2, 15));

    [Fact] void should_count_the_days_of_february() => _amount.ShouldEqual(new PriceInMinorUnits(350));
}
#endif
