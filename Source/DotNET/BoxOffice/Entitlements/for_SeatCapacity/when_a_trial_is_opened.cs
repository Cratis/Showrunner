// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_SeatCapacity;

public class when_a_trial_is_opened : a_tenant_read_model<SeatCapacity>
{
    SeatCapacity _capacity;

    async Task Because()
    {
        await Happened(new OperatorOpenedTrial(new(Tenants.Day(14)), 3));
        _capacity = _scenario.Instance!;
    }

    [Fact] void should_have_the_trial_seats() => _capacity.On(Tenants.Today).ShouldEqual(3);
    [Fact] void should_have_them_on_the_last_day() => _capacity.On(Tenants.Day(14)).ShouldEqual(3);
    [Fact] void should_have_none_after_the_last_day_without_any_event() => _capacity.On(Tenants.Day(15)).ShouldEqual(0);
    [Fact] void should_not_be_over_while_it_runs() => _capacity.IsTrialOver(Tenants.Day(14)).ShouldBeFalse();
    [Fact] void should_be_over_after_its_last_day() => _capacity.IsTrialOver(Tenants.Day(15)).ShouldBeTrue();
}
#endif
