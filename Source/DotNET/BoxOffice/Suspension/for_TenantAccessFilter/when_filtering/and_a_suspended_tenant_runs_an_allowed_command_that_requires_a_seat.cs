// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Seats;
using Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.given;

namespace Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.when_filtering;

public class and_a_suspended_tenant_runs_an_allowed_command_that_requires_a_seat : a_tenant_access_filter
{
    void Establish()
    {
        _entitlements.WhyReadOnly().Returns(ReadOnlyReason.TenantSuspended);
        _holder.Current.Returns(SeatHolder.Of(Tenants.Ada));
        _entitlements.HasSeat(Arg.Any<SeatHolder>()).Returns(false);
    }

    async Task Because() => await Filter(typeof(ExportingData));

    [Fact] void should_still_require_the_seat() => _result.AuthorizationFailureReason.ShouldEqual("SeatRequired");
}
#endif
