// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantStanding;

public class when_the_frontend_asks_why_the_tenant_is_read_only : Specification
{
    TenantStanding _suspended;
    TenantStanding _unsuspended;

    async Task Because()
    {
        var readModels = Substitute.For<IReadModels>();
        var entitlements = Substitute.For<IEntitlements>();
        entitlements.WhyReadOnly().Returns(ReadOnlyReason.TenantSuspended);
        readModels.GetInstanceById<TenantStanding>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns(
            new TenantStanding(TenantStream.Id.Value, true, SuspensionCause.PaymentOverdue, Tenants.At(1), ReadOnlyReason.None));
        _suspended = await TenantStanding.CurrentStanding(entitlements, readModels);

        readModels.GetInstanceById<TenantStanding>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns((TenantStanding)null!);
        entitlements.WhyReadOnly().Returns(ReadOnlyReason.None);
        _unsuspended = await TenantStanding.CurrentStanding(entitlements, readModels);
    }

    [Fact] void should_tell_why() => _suspended.ReadOnlyReason.ShouldEqual(ReadOnlyReason.TenantSuspended);
    [Fact] void should_tell_the_cause_the_operator_gave() => _suspended.Cause.ShouldEqual(SuspensionCause.PaymentOverdue);
    [Fact] void should_not_be_suspended_when_nothing_was_recorded() => _unsuspended.Suspended.ShouldBeFalse();
    [Fact] void should_not_be_read_only_when_nothing_was_recorded() => _unsuspended.ReadOnlyReason.ShouldEqual(ReadOnlyReason.None);
}
#endif
