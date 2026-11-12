// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListSubscriptions;

/// <summary>
/// What GuestList connects to an operator store is named for the part, so it does not collide with what another part
/// connects to the same store; and it receives the two public events and nothing else.
/// </summary>
public class when_naming_what_it_connects : Specification
{
    static readonly EventStoreName Operator = new("Direct");

    [Fact] void should_prefix_the_subscription_with_the_part() => GuestListSubscriptions.SubscriptionFor(Operator).Value.ShouldEqual("guestlist-Direct");
    [Fact] void should_observe_the_inbox_of_the_operator_store() => GuestListSubscriptions.SequenceFor(Operator).Value.ShouldEqual("inbox-Direct");
    [Fact] void should_name_the_inbox_reactor_for_the_operator_store() => GuestListSubscriptions.InboxFor(Operator).Value.ShouldEqual("Cratis.Showrunner.GuestList.Receiving.GuestListInbox.Direct");
    [Fact] void should_receive_the_two_public_events() => GuestListSubscriptions.PublicEvents.ShouldContainOnly(typeof(AudienceGranted), typeof(AudienceWithdrawn));
}
#endif
