// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.Availability.for_AudienceDelivery.when_reducing;

/// <summary>
/// Production received the decisions taken at minutes 10 and 20, and Staging only the one at minute 10. A decision taken at
/// minute 30 waits in both; the one at minute 15 waits only in Staging.
/// </summary>
public class receipts_in_two_environments : a_product_that_reports<AudienceDelivery>
{
    EnvironmentDelivery _production;
    EnvironmentDelivery _staging;

    async Task Because()
    {
        await Happened(Received(Production, At(10)), Received(Staging, At(10)), Received(Production, At(20)));
        _production = Built.Environments.Single(_ => _.Environment == Production);
        _staging = Built.Environments.Single(_ => _.Environment == Staging);
    }

    [Fact] void should_hold_each_environment() => Built.Environments.Count().ShouldEqual(2);
    [Fact] void should_have_received_through_the_newest_decision() => _production.ReceivedThrough.ShouldEqual(At(20));
    [Fact] void should_say_when_it_last_received() => _production.LastReceivedAt.ShouldEqual(At(2));
    [Fact] void should_keep_staging_apart() => _staging.ReceivedThrough.ShouldEqual(At(10));
    [Fact] void should_not_wait_for_what_it_received() => _production.IsWaiting(At(15)).ShouldBeFalse();
    [Fact] void should_wait_for_what_it_did_not_receive() => _staging.IsWaiting(At(15)).ShouldBeTrue();
    [Fact] void should_wait_for_a_later_decision_in_both() => new[] { _production, _staging }.All(_ => _.IsWaiting(At(30))).ShouldBeTrue();
}
#endif
