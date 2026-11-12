// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Holding.for_AudienceHoldings.when_reducing;

/// <summary>
/// A withdrawal removes the audience, and only the audience it names.
/// </summary>
public class a_withdrawn_audience : a_holding
{
    async Task Because() => await Recorded(
        GuestListStreams.Tenant,
        (0, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)),
        (1, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Preview)),
        (2, new AudienceWithdrawalRecorded(AudienceLevel.Tenant, Guests.Plus)));

    [Fact] void should_hold_only_the_other_audience() => Holdings.HeldAudiences().ShouldContainOnly(Guests.Preview);
}
#endif
