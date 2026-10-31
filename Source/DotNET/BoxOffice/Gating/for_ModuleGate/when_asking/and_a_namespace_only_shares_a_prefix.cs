// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.given;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.when_asking;

public class and_a_namespace_only_shares_a_prefix : a_module_gate
{
    ModuleId? _refused;

    async Task Because() => _refused = await _gate.RefusedModuleOf("Acme.AnalyticsExtras");

    [Fact] void should_not_count_it_as_the_modules() => _refused.ShouldBeNull();
}
#endif
