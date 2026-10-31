// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.given;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.when_asking;

public class and_the_module_is_only_previewed : a_module_gate
{
    ModuleId? _refused;

    async Task Because()
    {
        Given(new OperatorOpenedModulePreview("reports", new(Tenants.Day(7))));
        _refused = await _gate.RefusedModuleOf(Reports);
    }

    [Fact] void should_refuse_because_a_preview_is_not_use() => _refused.ShouldEqual(new ModuleId("reports"));
}
#endif
