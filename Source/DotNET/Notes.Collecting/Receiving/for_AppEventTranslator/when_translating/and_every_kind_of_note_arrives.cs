// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

public class and_every_kind_of_note_arrives : an_app_event_translator
{
    async Task Because()
    {
        var tenant = new TenantId(Tenant.Value);
        var application = ReceivedNotes.Application;
        var giver = ReceivedNotes.Giver;
        var context = ReceivedNotes.Context;
        await _translator.Translate(new NoteOnSlicesGiven(tenant, application, giver, context, [ReceivedNotes.Location], NoteKind.Suggestion, "Merge."), InboxContext(0), Application);
        await _translator.Translate(new NoteOnScreenGiven(tenant, application, giver, context, NoteKind.Question, "Why?"), InboxContext(1), Application);
        await _translator.Translate(new NoteOnComponentGiven(tenant, application, giver, context, ReceivedNotes.Component, NoteKind.Praise, "Nice."), InboxContext(2), Application);
        await _translator.Translate(new ComponentPropertyChangeSuggested(tenant, application, giver, context, ReceivedNotes.Component, "label", "Send", "Send invoice"), InboxContext(3), Application);
        await _translator.Translate(new NoteWithdrawn(tenant), InboxContext(4), Application);
        await ReadRecorded();
    }

    [Fact] void should_record_each_as_its_own_fact() => _recorded.Select(_ => _.Content.GetType()).ShouldContainOnly(
        typeof(NoteOnSlicesReceived),
        typeof(NoteOnScreenReceived),
        typeof(NoteOnComponentReceived),
        typeof(ComponentPropertyChangeReceived),
        typeof(NoteWithdrawalReceived));
}
#endif
