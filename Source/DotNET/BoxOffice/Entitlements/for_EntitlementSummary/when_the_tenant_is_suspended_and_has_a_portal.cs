// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_EntitlementSummary;

public class when_the_tenant_is_suspended_and_has_a_portal : a_summary_over_entitlements
{
    EntitlementSummary _summary;
    EntitlementSummary _withoutPortal;

    async Task Because()
    {
        Given(new OperatorSuspendedTenant(SuspensionCause.PaymentOverdue));
        _portal.IsAvailable.Returns(true);
        _summary = await Summarize();

        _portal.IsAvailable.Returns(false);
        _withoutPortal = await Summarize();
    }

    [Fact] void should_be_read_only_because_it_is_suspended() => _summary.ReadOnlyReason.ShouldEqual(ReadOnlyReason.TenantSuspended);
    [Fact] void should_tell_a_portal_can_be_opened() => _summary.CanOpenPortal.ShouldBeTrue();
    [Fact] void should_tell_when_there_is_no_portal() => _withoutPortal.CanOpenPortal.ShouldBeFalse();
}
#endif
