// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_the_tenant_is_suspended : some_entitlements
{
    bool _active;
    ReadOnlyReason _reason;

    async Task Because()
    {
        Given(new OperatorSuspendedTenant(SuspensionCause.PaymentOverdue));
        _active = await _entitlements.IsActive();
        _reason = await _entitlements.WhyReadOnly();
    }

    [Fact] void should_not_be_active() => _active.ShouldBeFalse();
    [Fact] void should_be_read_only_because_it_is_suspended() => _reason.ShouldEqual(ReadOnlyReason.TenantSuspended);
}
#endif
