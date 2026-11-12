// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Holding.for_AudienceHoldings.when_reducing;

/// <summary>
/// When two decisions carry the same time, the one recorded last wins.
/// </summary>
public class two_decisions_at_the_same_time : a_holding
{
    async Task Because() => await Recorded(
        GuestListStreams.Tenant,
        (3, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)),
        (3, new AudienceWithdrawalRecorded(AudienceLevel.Tenant, Guests.Plus)));

    [Fact] void should_not_hold_the_audience() => Holdings.HeldAudiences().ShouldBeEmpty();
}
#endif
