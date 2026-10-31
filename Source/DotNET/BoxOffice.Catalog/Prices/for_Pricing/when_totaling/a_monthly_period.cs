// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_totaling;

public class a_monthly_period : Specification
{
    PriceInMinorUnits _amount;

    void Because() => _amount = Pricing.TotalFor(10000, 20m, BillingInterval.Monthly);

    [Fact] void should_be_the_monthly_price() => _amount.ShouldEqual(new PriceInMinorUnits(10000));
}
#endif
