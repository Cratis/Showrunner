// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_totaling;

public class a_yearly_period_without_discount : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.TotalFor(1000, 0m, BillingInterval.Yearly);

    [Fact] void should_be_twelve_months() => _amount.ShouldEqual(new PriceInMinorUnits(12000));
}
#endif
