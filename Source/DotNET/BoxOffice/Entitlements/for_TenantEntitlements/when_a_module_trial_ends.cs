// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_a_module_trial_ends : some_entitlements
{
    bool _duringTheTrial;
    bool _afterTheTrial;

    async Task Because()
    {
        Given(new OperatorGrantedModuleTrial("analytics", new(Tenants.Today)));
        _duringTheTrial = await _entitlements.Has("analytics");

        // No event happens: the day after the trial's last day arrives.
        _clock.MoveTo(Tenants.Day(1));
        _afterTheTrial = await _entitlements.Has("analytics");
    }

    [Fact] void should_admit_the_module_until_the_last_day() => _duringTheTrial.ShouldBeTrue();
    [Fact] void should_stop_admitting_it_without_any_event() => _afterTheTrial.ShouldBeFalse();
}
#endif
