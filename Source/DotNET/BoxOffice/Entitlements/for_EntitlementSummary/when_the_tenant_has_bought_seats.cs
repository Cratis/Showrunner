// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_EntitlementSummary;

public class when_the_tenant_has_bought_seats : a_summary_over_entitlements
{
    EntitlementSummary _summary;
    EntitlementSummary _unending;

    async Task Because()
    {
        Given(
            new OperatorOpenedTrial(new(Tenants.Day(-5)), 3),
            new OperatorSetSeatCapacity(10, new(Tenants.Day(30)), SeatCapacitySource.Purchase));
        _summary = await Summarize();

        Given(new OperatorSetSeatCapacity(10, EntitlementEnd.Never, SeatCapacitySource.Purchase));
        _unending = await Summarize();
    }

    [Fact] void should_no_longer_be_a_trial() => _summary.IsTrial.ShouldBeFalse();
    [Fact] void should_have_no_trial_end() => _summary.TrialEnds.ShouldBeNull();
    [Fact] void should_have_the_purchased_seats() => _summary.Seats.ShouldEqual(10);
    [Fact] void should_tell_the_last_day_of_the_seats() => _summary.SeatsUntil.ShouldEqual(Tenants.Day(30));
    [Fact] void should_tell_no_end_for_seats_that_do_not_end() => _unending.SeatsUntil.ShouldBeNull();
}
#endif
