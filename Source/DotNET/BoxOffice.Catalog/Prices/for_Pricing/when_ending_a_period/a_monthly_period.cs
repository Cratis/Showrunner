// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_ending_a_period;

public class a_monthly_period : Specification
{
    DateOnly _end;

    void Because() => _end = Pricing.PeriodEnd(new DateOnly(2026, 6, 16), BillingInterval.Monthly);

    [Fact] void should_be_the_day_the_next_period_starts() => _end.ShouldEqual(new DateOnly(2026, 7, 16));
}
#endif
