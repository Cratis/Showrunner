// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_the_tenant_is_reinstated : some_entitlements
{
    bool _active;

    async Task Because()
    {
        Given(new OperatorSuspendedTenant(SuspensionCause.PaymentOverdue), new OperatorReinstatedTenant());
        _active = await _entitlements.IsActive();
    }

    [Fact] void should_be_active_at_once() => _active.ShouldBeTrue();
}
#endif
