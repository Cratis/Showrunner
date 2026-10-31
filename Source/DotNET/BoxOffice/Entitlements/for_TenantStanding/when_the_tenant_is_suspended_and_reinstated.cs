// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantStanding;

public class when_the_tenant_is_suspended_and_reinstated : a_tenant_read_model<TenantStanding>
{
    TenantStanding _suspended;
    TenantStanding _reinstated;

    async Task Because()
    {
        await Happened(new OperatorSuspendedTenant(SuspensionCause.PaymentOverdue));
        _suspended = _scenario.Instance!;
        await Happened(new OperatorReinstatedTenant());
        _reinstated = _scenario.Instance!;
    }

    [Fact] void should_be_suspended() => _suspended.Suspended.ShouldBeTrue();
    [Fact] void should_say_why() => _suspended.Cause.ShouldEqual(SuspensionCause.PaymentOverdue);
    [Fact] void should_lift_the_suspension_at_once() => _reinstated.Suspended.ShouldBeFalse();
}
#endif
