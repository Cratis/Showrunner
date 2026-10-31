// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_the_trial_has_ended_without_seats : some_entitlements
{
    ReadOnlyReason _whileRunning;
    ReadOnlyReason _afterTheEnd;

    async Task Because()
    {
        Given(new OperatorOpenedTrial(new(Tenants.Day(3)), 3));
        _clock.MoveTo(Tenants.Day(3));
        _whileRunning = await _entitlements.WhyReadOnly();

        // No event happens: the trial's last day passes.
        _clock.MoveTo(Tenants.Day(4));
        _afterTheEnd = await _entitlements.WhyReadOnly();
    }

    [Fact] void should_be_active_on_the_last_day() => _whileRunning.ShouldEqual(ReadOnlyReason.None);
    [Fact] void should_be_read_only_because_the_trial_closed_the_day_after() => _afterTheEnd.ShouldEqual(ReadOnlyReason.TrialClosed);
}
#endif
