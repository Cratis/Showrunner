// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_SeatCapacity;

public class when_purchased_seats_end : a_tenant_read_model<SeatCapacity>
{
    SeatCapacity _capacity;

    async Task Because()
    {
        await Happened(new OperatorSetSeatCapacity(10, new(Tenants.Day(30)), SeatCapacitySource.Purchase));
        _capacity = _scenario.Instance!;
    }

    [Fact] void should_have_the_seats_until_the_last_day() => _capacity.On(Tenants.Day(30)).ShouldEqual(10);
    [Fact] void should_have_none_after_the_last_day_without_any_event() => _capacity.On(Tenants.Day(31)).ShouldEqual(0);
    [Fact] void should_not_be_a_trial_that_is_over() => _capacity.IsTrialOver(Tenants.Day(31)).ShouldBeFalse();
}
#endif
