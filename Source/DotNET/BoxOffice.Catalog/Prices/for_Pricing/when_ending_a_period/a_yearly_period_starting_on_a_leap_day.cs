// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.for_Pricing.when_ending_a_period;

public class a_yearly_period_starting_on_a_leap_day : Specification
{
    DateOnly _end;

    void Because() => _end = Pricing.PeriodEnd(new DateOnly(2028, 2, 29), BillingInterval.Yearly);

    [Fact] void should_be_the_day_the_next_period_starts() => _end.ShouldEqual(new DateOnly(2029, 2, 28));
}
#endif
