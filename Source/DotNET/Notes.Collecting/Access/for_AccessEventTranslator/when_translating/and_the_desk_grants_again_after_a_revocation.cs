// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.given;
using Cratis.Showrunner.Notes.Collecting.Access.given;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.when_translating;

/// <summary>
/// The desk grants, revokes and grants again, each at its own time: all three are decisions of their own and are recorded, so the latest stands.
/// </summary>
public class and_the_desk_grants_again_after_a_revocation : an_access_event_translator
{
    async Task Because()
    {
        await _translator.Translate(AccessEvents.PublicGranted(), InboxContext(0), Desk);
        await _translator.Translate(AccessEvents.PublicRevoked(), InboxContext(1, Occurred.AddMinutes(1)), Desk);
        await _translator.Translate(AccessEvents.PublicGranted(), InboxContext(2, Occurred.AddMinutes(2)), Desk);
        await ReadRecorded(AccessEvents.Giver);
    }

    [Fact] void should_record_every_decision() => _recorded.Count.ShouldEqual(3);
    [Fact] void should_end_with_the_grant() => _recorded[^1].Content.ShouldBeOfExactType<FeedbackAccessGrantReceived>();
}
#endif
