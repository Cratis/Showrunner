// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Starting;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_ModuleEntitlements;

public class when_a_tenant_starts_on_the_default_offering : a_tenant_read_model<ModuleEntitlements>
{
    ModuleEntitlements _entitlements;
    ModuleEntitlements _afterRevoking;

    async Task Because()
    {
        await Happened(new TenantStartedOnDefaultOffering("Starter", 3, [new ModuleId("core"), new ModuleId("analytics")]));
        _entitlements = _scenario.Instance!;
        await Happened(new OperatorRevokedModule(new ModuleId("analytics")));
        _afterRevoking = _scenario.Instance!;
    }

    [Fact] void should_have_every_module_of_the_offer_in_full() => _entitlements.AccessTo(new ModuleId("analytics"), Tenants.Today).ShouldEqual(ModuleAccess.Full);
    [Fact] void should_have_the_other_module_too() => _entitlements.AccessTo(new ModuleId("core"), Tenants.Today).ShouldEqual(ModuleAccess.Full);
    [Fact] void should_not_have_a_module_the_offer_leaves_out() => _entitlements.AccessTo(new ModuleId("exports"), Tenants.Today).ShouldEqual(ModuleAccess.None);
    [Fact] void should_lose_a_module_the_operator_revokes_later() => _afterRevoking.AccessTo(new ModuleId("analytics"), Tenants.Today).ShouldEqual(ModuleAccess.None);
    [Fact] void should_keep_the_others() => _afterRevoking.AccessTo(new ModuleId("core"), Tenants.Today).ShouldEqual(ModuleAccess.Full);
}
#endif
