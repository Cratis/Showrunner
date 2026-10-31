// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_SeatCapacity;

public class when_the_trial_closes : a_tenant_read_model<SeatCapacity>
{
    SeatCapacity _capacity;

    async Task Because()
    {
        await Happened(new OperatorOpenedTrial(new(Tenants.Day(14)), 3), new OperatorClosedTrial(TrialClosure.EndedEarly));
        _capacity = _scenario.Instance!;
    }

    [Fact] void should_have_no_seats_before_its_last_day() => _capacity.On(Tenants.Today).ShouldEqual(0);
    [Fact] void should_be_over() => _capacity.IsTrialOver(Tenants.Today).ShouldBeTrue();
}
#endif
