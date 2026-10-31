// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Seats.for_SeatStanding;

public class when_asking_where_the_user_stands : Specification
{
    static readonly SeatRoster _roster = new(
        TenantStream.Id.Value,
        [new(Tenants.Ada, Tenants.At(0)), new(Tenants.Bo, Tenants.At(1)), new(Tenants.Cy, Tenants.At(2))]);

    SeatStanding _licensed;
    SeatStanding _overCapacity;
    SeatStanding _noSeat;
    SeatStanding _nobody;

    async Task Because()
    {
        var readModels = Substitute.For<IReadModels>();
        readModels.GetInstanceById<SeatRoster>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns(_roster);
        var entitlements = Substitute.For<IEntitlements>();
        entitlements.SeatCapacity().Returns(2);
        entitlements.WhyReadOnly().Returns(ReadOnlyReason.None);

        _licensed = await SeatStanding.Mine(readModels, entitlements, Holder(Tenants.Ada));
        _overCapacity = await SeatStanding.Mine(readModels, entitlements, Holder(Tenants.Cy));
        _noSeat = await SeatStanding.Mine(readModels, entitlements, Holder(Tenants.Person("9", "Dee", "dee@fjord.example.com")));
        _nobody = await SeatStanding.Mine(readModels, entitlements, Holder(null));
    }

    [Fact] void should_be_licensed_when_the_seat_is_covered() => _licensed.Status.ShouldEqual(SeatStatus.Licensed);
    [Fact] void should_be_over_capacity_when_the_seat_is_held_but_not_covered() => _overCapacity.Status.ShouldEqual(SeatStatus.OverCapacity);
    [Fact] void should_have_no_seat_without_one() => _noSeat.Status.ShouldEqual(SeatStatus.NoSeat);
    [Fact] void should_have_no_seat_when_nobody_is_signed_in() => _nobody.Status.ShouldEqual(SeatStatus.NoSeat);
    [Fact] void should_tell_how_many_seats_the_tenant_may_use() => _licensed.Capacity.ShouldEqual(2);
    [Fact] void should_tell_how_many_are_held() => _licensed.Held.ShouldEqual(3);

    static ICurrentSeatHolder Holder(Person? person)
    {
        var holder = Substitute.For<ICurrentSeatHolder>();
        holder.Current.Returns(person is null ? null : SeatHolder.Of(person));
        return holder;
    }
}
#endif
