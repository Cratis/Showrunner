// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

public class and_a_note_arrives_for_a_tenant : an_app_event_translator
{
    NoteOnContextReceived _note;

    async Task Because()
    {
        await _translator.Translate(Given(Tenant.Value), InboxContext(), Application);
        await ReadRecorded();
        _note = _recorded.Single().Content as NoteOnContextReceived;
    }

    [Fact] void should_record_it_in_the_tenant() => _recorded.Count.ShouldEqual(1);
    [Fact] void should_record_it_as_received() => _note.ShouldNotBeNull();
    [Fact] void should_say_which_application_store_it_came_from() => _note.AppStore.ShouldEqual(new AppStoreName("Ledger"));
    [Fact] void should_keep_the_giver() => _note.Giver.ShouldEqual(Given(Tenant.Value).Giver);
    [Fact] void should_keep_the_text() => _note.Text.ShouldEqual(Given(Tenant.Value).Text);
    [Fact] void should_keep_the_kind() => _note.Kind.ShouldEqual(NoteKind.Problem);
    [Fact] void should_keep_the_application() => _note.Application.ShouldEqual(Given(Tenant.Value).Application);
    [Fact] void should_keep_when_it_happened() => _recorded.Single().Context.Occurred.ShouldEqual(Occurred);
    [Fact] void should_keep_the_correlation_id() => _recorded.Single().Context.CorrelationId.ShouldEqual(_correlationId);
    [Fact] void should_keep_the_compliance_subject() => _recorded.Single().Context.Subject.ShouldEqual(new Subject("notes-giver-abc"));
}
#endif
