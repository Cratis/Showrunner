// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_ModuleEntitlements;

public class when_a_module_is_granted : a_tenant_read_model<ModuleEntitlements>
{
    ModuleEntitlements _entitlements;

    async Task Because()
    {
        await Happened(new OperatorGrantedModule("analytics", new(Tenants.Day(30))));
        _entitlements = _scenario.Instance!;
    }

    [Fact] void should_be_had_in_full() => _entitlements.AccessTo("analytics", Tenants.Today).ShouldEqual(ModuleAccess.Full);
    [Fact] void should_be_had_on_its_last_day() => _entitlements.AccessTo("analytics", Tenants.Day(30)).ShouldEqual(ModuleAccess.Full);
    [Fact] void should_end_the_day_after_its_last_day_without_any_event() => _entitlements.AccessTo("analytics", Tenants.Day(31)).ShouldEqual(ModuleAccess.None);
    [Fact] void should_not_give_other_modules() => _entitlements.AccessTo("billing", Tenants.Today).ShouldEqual(ModuleAccess.None);
}
#endif
