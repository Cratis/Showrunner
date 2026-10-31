// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_ModuleEntitlements;

public class when_a_grant_arrives_after_the_revoke_that_followed_it : a_tenant_read_model<ModuleEntitlements>
{
    ModuleEntitlements _entitlements;

    async Task Because()
    {
        // The operator granted the module, then revoked it; a replay delivers the revoke first.
        await Arrived(
            (Tenants.At(20), new OperatorRevokedModule("analytics")),
            (Tenants.At(10), new OperatorGrantedModule("analytics", EntitlementEnd.Never)));
        _entitlements = _scenario.Instance!;
    }

    [Fact] void should_end_in_the_revoked_state() => _entitlements.AccessTo("analytics", Tenants.Today).ShouldEqual(ModuleAccess.None);
}
#endif
