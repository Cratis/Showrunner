// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Holding.for_AudienceHoldings.when_reducing;

/// <summary>
/// A grant decided after a withdrawal brings the audience back: the newest decision wins.
/// </summary>
public class an_audience_granted_again_after_a_withdrawal : a_holding
{
    async Task Because() => await Recorded(
        GuestListStreams.Tenant,
        (0, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)),
        (1, new AudienceWithdrawalRecorded(AudienceLevel.Tenant, Guests.Plus)),
        (2, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)));

    [Fact] void should_hold_the_audience() => Holdings.HeldAudiences().ShouldContainOnly(Guests.Plus);
}
#endif
