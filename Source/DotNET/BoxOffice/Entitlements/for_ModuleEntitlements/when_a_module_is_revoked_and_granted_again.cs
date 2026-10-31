// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_ModuleEntitlements;

public class when_a_module_is_revoked_and_granted_again : a_tenant_read_model<ModuleEntitlements>
{
    ModuleEntitlements _entitlements;

    async Task Because()
    {
        await Arrived(
            (Tenants.At(30), new OperatorGrantedModule("analytics", EntitlementEnd.Never)),
            (Tenants.At(10), new OperatorGrantedModule("analytics", EntitlementEnd.Never)),
            (Tenants.At(20), new OperatorRevokedModule("analytics")));
        _entitlements = _scenario.Instance!;
    }

    [Fact] void should_have_the_newest_decision() => _entitlements.AccessTo("analytics", Tenants.Today).ShouldEqual(ModuleAccess.Full);
    [Fact] void should_keep_one_entry_for_the_module() => _entitlements.Modules.Count().ShouldEqual(1);
}
#endif
