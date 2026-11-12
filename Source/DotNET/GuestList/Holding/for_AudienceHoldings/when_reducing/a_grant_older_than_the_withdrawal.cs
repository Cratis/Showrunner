// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Holding.for_AudienceHoldings.when_reducing;

/// <summary>
/// The newest decision wins whatever order the decisions arrive in: a grant that arrives after the withdrawal, but was
/// decided before it, does not bring the audience back.
/// </summary>
public class a_grant_older_than_the_withdrawal : a_holding
{
    async Task Because() => await Recorded(
        GuestListStreams.Tenant,
        (5, new AudienceWithdrawalRecorded(AudienceLevel.Tenant, Guests.Plus)),
        (1, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)));

    [Fact] void should_not_hold_the_audience() => Holdings.HeldAudiences().ShouldBeEmpty();
    [Fact] void should_remember_the_withdrawal() => Holdings.Decisions.Single().Granted.ShouldBeFalse();
}
#endif
