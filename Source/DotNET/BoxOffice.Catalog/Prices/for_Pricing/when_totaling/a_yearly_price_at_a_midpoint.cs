// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_totaling;

public class a_yearly_price_at_a_midpoint : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.TotalFor(1, 62.5m, BillingInterval.Yearly);

    [Fact] void should_round_away_from_zero() => _amount.ShouldEqual(new PriceInMinorUnits(5));
}
#endif
