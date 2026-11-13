// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.Availability.for_AudienceDelivery.when_reducing;

/// <summary>
/// A receipt for an older decision never moves the high-water mark back.
/// </summary>
public class a_receipt_that_arrives_late : a_product_that_reports<AudienceDelivery>
{
    async Task Because() => await Happened(Received(Production, At(20)), Received(Production, At(10)));

    [Fact] void should_keep_the_newest_decision() => Built.Environments.Single().ReceivedThrough.ShouldEqual(At(20));
    [Fact] void should_say_when_it_last_received() => Built.Environments.Single().LastReceivedAt.ShouldEqual(At(1));
}
#endif
