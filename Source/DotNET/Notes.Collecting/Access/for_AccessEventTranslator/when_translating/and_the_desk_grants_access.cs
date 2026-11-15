// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.given;
using Cratis.Showrunner.Notes.Collecting.Access.given;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.when_translating;

/// <summary>
/// The desk grants a user who has never given a note the right: it is recorded under the user's id, in the tenant's namespace, with the user as the compliance subject.
/// </summary>
public class and_the_desk_grants_access : an_access_event_translator
{
    async Task Because()
    {
        await _translator.Translate(AccessEvents.PublicGranted(), InboxContext(), Desk);
        await ReadRecorded(AccessEvents.Giver);
    }

    [Fact] void should_record_the_grant() => _recorded.Select(_ => _.Content).OfType<FeedbackAccessGrantReceived>().Single().GrantedBy.ShouldEqual(AccessEvents.Grantor);
    [Fact] void should_keep_when_the_desk_decided() => _recorded.Single().Context.Occurred.ShouldEqual(Occurred);
    [Fact] void should_record_it_for_the_user_not_for_the_desks_event_source() => _recorded.Single().Context.EventSourceId.ShouldEqual((EventSourceId)FeedbackGiverId.For(AccessEvents.Giver));
    [Fact] void should_record_it_with_the_user_as_the_subject() => _recorded.Single().Context.Subject.ShouldEqual(FeedbackGiverId.SubjectOf(AccessEvents.Giver));
}
#endif
