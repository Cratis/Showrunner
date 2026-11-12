// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Holding.for_AudienceHoldings.when_reducing;

/// <summary>
/// A withdrawal that arrives after a newer grant, but was decided before it, does not remove the audience.
/// </summary>
public class a_withdrawal_older_than_the_grant : a_holding
{
    async Task Because() => await Recorded(
        GuestListStreams.Tenant,
        (5, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)),
        (1, new AudienceWithdrawalRecorded(AudienceLevel.Tenant, Guests.Plus)));

    [Fact] void should_hold_the_audience() => Holdings.HeldAudiences().ShouldContainOnly(Guests.Plus);
}
#endif
