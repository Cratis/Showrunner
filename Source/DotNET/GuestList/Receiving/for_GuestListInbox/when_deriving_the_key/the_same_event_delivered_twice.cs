// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_deriving_the_key;

/// <summary>
/// Every copy of an event has the same key, and a different decision, time or operator store has another.
/// </summary>
public class the_same_event_delivered_twice : a_guest_list_inbox
{
    IdempotencyKey _first;
    IdempotencyKey _again;
    IdempotencyKey _otherDecision;
    IdempotencyKey _otherTime;

    void Because()
    {
        var context = NextContext();
        _first = GuestListInbox.KeyFor(Guests.GrantToTenant(Guests.Plus), context, OperatorStore);
        _again = GuestListInbox.KeyFor(Guests.GrantToTenant(Guests.Plus), context, OperatorStore);
        _otherDecision = GuestListInbox.KeyFor(Guests.WithdrawFromTenant(Guests.Plus), context, OperatorStore);
        _otherTime = GuestListInbox.KeyFor(Guests.GrantToTenant(Guests.Plus), NextContext(), OperatorStore);
    }

    [Fact] void should_have_the_same_key_for_every_copy() => _again.ShouldEqual(_first);
    [Fact] void should_have_another_key_for_another_decision() => _otherDecision.ShouldNotEqual(_first);
    [Fact] void should_have_another_key_for_another_time() => _otherTime.ShouldNotEqual(_first);
}
#endif
