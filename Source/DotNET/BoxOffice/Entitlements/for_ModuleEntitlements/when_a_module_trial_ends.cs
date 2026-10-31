// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_ModuleEntitlements;

public class when_a_module_trial_ends : a_tenant_read_model<ModuleEntitlements>
{
    ModuleEntitlements _entitlements;

    async Task Because()
    {
        await Happened(new OperatorGrantedModuleTrial("analytics", new(Tenants.Day(3))));
        _entitlements = _scenario.Instance!;
    }

    [Fact] void should_be_a_trial_while_it_runs() => _entitlements.AccessTo("analytics", Tenants.Day(3)).ShouldEqual(ModuleAccess.Trial);
    [Fact] void should_stop_admitting_the_day_after_its_end_without_any_event() => _entitlements.AccessTo("analytics", Tenants.Day(4)).ShouldEqual(ModuleAccess.None);
}
#endif
