// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Holding.for_AudienceHoldings.when_reducing;

/// <summary>
/// Granting an audience that is already held leaves it held once.
/// </summary>
public class an_audience_granted_twice : a_holding
{
    async Task Because() => await Recorded(
        GuestListStreams.Tenant,
        (0, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)),
        (1, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)));

    [Fact] void should_hold_the_audience_once() => Holdings.HeldAudiences().ShouldContainOnly(Guests.Plus);
    [Fact] void should_keep_one_decision() => Holdings.Decisions.Count().ShouldEqual(1);
}
#endif
