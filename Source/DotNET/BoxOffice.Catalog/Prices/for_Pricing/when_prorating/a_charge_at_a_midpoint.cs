// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_prorating;

public class a_charge_at_a_midpoint : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.Prorate(1, 1, 1, new DateOnly(2026, 7, 1), BillingInterval.Monthly, new DateOnly(2026, 6, 16));

    [Fact] void should_round_away_from_zero() => _amount.ShouldEqual(new PriceInMinorUnits(1));
}
#endif
