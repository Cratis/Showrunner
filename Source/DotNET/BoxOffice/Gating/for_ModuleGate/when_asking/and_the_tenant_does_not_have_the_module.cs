// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Acme.Analytics;
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.given;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.when_asking;

public class and_the_tenant_does_not_have_the_module : a_module_gate
{
    ModuleId? _refused;
    bool _admits;
    bool _admitsTheModulesReactor;

    async Task Because()
    {
        _refused = await _gate.RefusedModuleOf($"{Analytics}.Reports");
        _admits = await _gate.Admits(typeof(a_module_gate));
        _admitsTheModulesReactor = await _gate.Admits(typeof(AnAnalyticsReactor));
    }

    [Fact] void should_refuse_because_of_the_module() => _refused.ShouldEqual(new ModuleId("analytics"));
    [Fact] void should_admit_what_no_module_owns() => _admits.ShouldBeTrue();
    [Fact] void should_not_admit_a_reactor_of_the_module() => _admitsTheModulesReactor.ShouldBeFalse();
}
#endif
