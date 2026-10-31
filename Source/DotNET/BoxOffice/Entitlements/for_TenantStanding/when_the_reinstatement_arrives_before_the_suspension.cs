// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantStanding;

public class when_the_reinstatement_arrives_before_the_suspension : a_tenant_read_model<TenantStanding>
{
    TenantStanding _standing;

    async Task Because()
    {
        await Arrived(
            (Tenants.At(20), new OperatorReinstatedTenant()),
            (Tenants.At(10), new OperatorSuspendedTenant(SuspensionCause.AdministratorDecision)));
        _standing = _scenario.Instance!;
    }

    [Fact] void should_end_reinstated() => _standing.Suspended.ShouldBeFalse();
}
#endif
