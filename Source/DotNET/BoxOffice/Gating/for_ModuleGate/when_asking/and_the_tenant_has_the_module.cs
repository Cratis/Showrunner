// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Acme.Analytics;
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.given;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.when_asking;

public class and_the_tenant_has_the_module : a_module_gate
{
    ModuleId? _refused;
    bool _admits;

    async Task Because()
    {
        Given(new OperatorGrantedModule("analytics", EntitlementEnd.Never));
        _refused = await _gate.RefusedModuleOf($"{Analytics}.Reports");
        _admits = await _gate.Admits(typeof(AnAnalyticsReactor));
    }

    [Fact] void should_refuse_nothing() => _refused.ShouldBeNull();
    [Fact] void should_admit_what_the_module_owns() => _admits.ShouldBeTrue();
}

#endif
