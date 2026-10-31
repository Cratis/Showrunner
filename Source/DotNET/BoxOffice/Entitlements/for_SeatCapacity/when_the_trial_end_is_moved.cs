// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_SeatCapacity;

public class when_the_trial_end_is_moved : a_tenant_read_model<SeatCapacity>
{
    SeatCapacity _capacity;

    async Task Because()
    {
        // The move happened after the trial opened, but arrives first.
        await Arrived(
            (Tenants.At(20), new OperatorMovedTrialEnd(new(Tenants.Day(30)))),
            (Tenants.At(10), new OperatorOpenedTrial(new(Tenants.Day(14)), 3)));
        _capacity = _scenario.Instance!;
    }

    [Fact] void should_have_the_trial_seats_until_the_moved_end() => _capacity.On(Tenants.Day(30)).ShouldEqual(3);
    [Fact] void should_have_none_after_the_moved_end() => _capacity.On(Tenants.Day(31)).ShouldEqual(0);
}
#endif
