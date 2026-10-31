// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.given;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.when_asking;

public class and_the_trial_has_expired : a_module_gate
{
    ModuleId? _refused;
    ModuleId? _onTheLastDay;

    async Task Because()
    {
        Given(new OperatorGrantedModuleTrial("analytics", new(Tenants.Day(-1))));
        _refused = await _gate.RefusedModuleOf(Analytics);

        _clock.MoveTo(Tenants.Day(-1));
        _onTheLastDay = await _gate.RefusedModuleOf(Analytics);
    }

    [Fact] void should_refuse_because_of_the_module_without_any_event() => _refused.ShouldEqual(new ModuleId("analytics"));
    [Fact] void should_have_admitted_it_on_its_last_day() => _onTheLastDay.ShouldBeNull();
}
#endif
