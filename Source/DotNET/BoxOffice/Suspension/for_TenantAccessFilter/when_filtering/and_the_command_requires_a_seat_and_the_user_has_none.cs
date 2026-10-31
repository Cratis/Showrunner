// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Seats;
using Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.given;

namespace Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.when_filtering;

public class and_the_command_requires_a_seat_and_the_user_has_none : a_tenant_access_filter
{
    void Establish()
    {
        _holder.Current.Returns(SeatHolder.Of(Tenants.Ada));
        _entitlements.HasSeat(Arg.Any<SeatHolder>()).Returns(false);
    }

    async Task Because() => await Filter(typeof(ChangingSettings));

    [Fact] void should_refuse_the_command() => _result.ShouldNotBeAuthorized();
    [Fact] void should_say_a_seat_is_required() => _result.AuthorizationFailureReason.ShouldEqual("SeatRequired");
}
#endif
