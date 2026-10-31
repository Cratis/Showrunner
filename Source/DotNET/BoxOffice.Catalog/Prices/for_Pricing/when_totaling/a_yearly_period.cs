// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_totaling;

public class a_yearly_period : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.TotalFor(1000, 20m, BillingInterval.Yearly);

    [Fact] void should_be_twelve_months_less_the_discount() => _amount.ShouldEqual(new PriceInMinorUnits(9600));
}
#endif
