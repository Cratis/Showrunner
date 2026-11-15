// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Access.given;
using Cratis.Showrunner.Notes.Access.Receiving.for_AccessEventTranslator.given;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access.Receiving.for_AccessEventTranslator.when_translating;

/// <summary>
/// The desk grants and then revokes: both decisions are recorded, in the order the desk made them.
/// </summary>
public class and_the_desk_revokes_access : an_access_event_translator
{
    async Task Because()
    {
        await _translator.Translate(AccessEvents.PublicGranted(), InboxContext(0), Desk);
        await _translator.Translate(AccessEvents.PublicRevoked(), InboxContext(1, Occurred.AddMinutes(1)), Desk);
        await ReadRecorded(AccessEvents.Giver);
    }

    [Fact] void should_record_the_grant() => _recorded.Select(_ => _.Content).OfType<DeskGrantedFeedbackAccess>().Count().ShouldEqual(1);
    [Fact] void should_record_the_revocation() => _recorded.Select(_ => _.Content).OfType<DeskRevokedFeedbackAccess>().Single().RevokedBy.ShouldEqual(AccessEvents.Grantor);
    [Fact] void should_record_the_revocation_last() => _recorded[^1].Content.ShouldBeOfExactType<DeskRevokedFeedbackAccess>();
}
#endif
