// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_the_trial_has_ended_but_seats_were_granted : some_entitlements
{
    bool _active;

    async Task Because()
    {
        Given(
            new OperatorOpenedTrial(new(Tenants.Day(-5)), 3),
            new OperatorSetSeatCapacity(5, EntitlementEnd.Never, SeatCapacitySource.Grant),
            new OperatorClosedTrial(TrialClosure.Converted));
        _active = await _entitlements.IsActive();
    }

    [Fact] void should_not_be_read_only() => _active.ShouldBeTrue();
}
#endif
