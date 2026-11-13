// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Product.Availability.given;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_EntitlementsHostContextValues;

public class when_a_gate_reads_whether_the_tenant_is_active : a_tenant_with_entitlements
{
    AvailabilityResult _active;
    AvailabilityResult _readOnlyWhenSuspended;
    AvailabilityResult _inactiveWhenSuspended;

    async Task Because()
    {
        _active = await Evaluate("entitlements.active");
        _entitlements.WhyReadOnly().Returns(ReadOnlyReason.TenantSuspended);
        _inactiveWhenSuspended = await Evaluate("entitlements.active");
        _readOnlyWhenSuspended = await Evaluate("entitlements.readOnlyReason", ComparisonOperator.Equal, "TenantSuspended");
    }

    [Fact] void should_be_active_when_nothing_makes_it_read_only() => _active.IsAvailable.ShouldBeTrue();
    [Fact] void should_not_be_active_when_suspended() => _inactiveWhenSuspended.Outcome.ShouldEqual(AvailabilityOutcome.Unavailable);
    [Fact] void should_say_why_it_is_read_only() => _readOnlyWhenSuspended.IsAvailable.ShouldBeTrue();
}
#endif
