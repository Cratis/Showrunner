// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.given;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.when_asking;

public class and_the_module_was_revoked : a_module_gate
{
    ModuleId? _refused;

    async Task Because()
    {
        Given(
            new OperatorGrantedModule("analytics", EntitlementEnd.Never),
            new OperatorRevokedModule("analytics"));
        _refused = await _gate.RefusedModuleOf(Analytics);
    }

    [Fact] void should_refuse_because_of_the_module() => _refused.ShouldEqual(new ModuleId("analytics"));
}
#endif
