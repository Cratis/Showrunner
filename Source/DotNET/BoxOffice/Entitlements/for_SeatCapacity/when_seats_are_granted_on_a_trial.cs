// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_SeatCapacity;

public class when_seats_are_granted_on_a_trial : a_tenant_read_model<SeatCapacity>
{
    SeatCapacity _capacity;

    async Task Because()
    {
        await Happened(
            new OperatorOpenedTrial(new(Tenants.Day(14)), 3),
            new OperatorSetSeatCapacity(5, EntitlementEnd.Never, SeatCapacitySource.Grant),
            new OperatorClosedTrial(TrialClosure.Converted));
        _capacity = _scenario.Instance!;
    }

    [Fact] void should_replace_the_trial_seats_not_add_to_them() => _capacity.On(Tenants.Today).ShouldEqual(5);
    [Fact] void should_keep_the_seats_after_the_trial_end() => _capacity.On(Tenants.Day(100)).ShouldEqual(5);
    [Fact] void should_not_leave_the_tenant_with_an_over_trial() => _capacity.IsTrialOver(Tenants.Day(100)).ShouldBeFalse();
}
#endif
