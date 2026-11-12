// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Holding.for_AudienceHoldings.when_reducing;

/// <summary>
/// A granted audience is held at the level it was granted at.
/// </summary>
public class a_granted_audience : a_holding
{
    async Task Because() => await Recorded(GuestListStreams.Tenant, (0, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)));

    [Fact] void should_hold_the_audience() => Holdings.HeldAudiences().ShouldContainOnly(Guests.Plus);
    [Fact] void should_be_held_at_the_level() => Holdings.Level.ShouldEqual(AudienceLevel.Tenant);
    [Fact] void should_be_the_stream_it_was_recorded_in() => Holdings.Id.ShouldEqual(GuestListStreams.Tenant.Value);
}
#endif
