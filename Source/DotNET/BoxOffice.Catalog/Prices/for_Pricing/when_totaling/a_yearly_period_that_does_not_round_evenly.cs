// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_totaling;

public class a_yearly_period_that_does_not_round_evenly : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.TotalFor(999, 15m, BillingInterval.Yearly);

    [Fact] void should_round_to_a_whole_minor_unit() => _amount.ShouldEqual(new PriceInMinorUnits(10190));
}
#endif
