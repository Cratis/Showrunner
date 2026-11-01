// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

public class a_note_on_the_context : an_outbox_publisher
{
    GiverGaveNoteOnContext _given;
    NoteOnContextGiven _published;
    bool _eventLogHasEvents;

    async Task Because()
    {
        _given = NoteEvents.Given();
        var context = ContextFor();
        await _publisher.GaveNoteOnContext(_given, context, DeliveryOf(context));
        await ReadOutbox();
        _published = _outbox.Single().Content as NoteOnContextGiven;
        _eventLogHasEvents = await _eventStore.GetEventSequence(EventSequenceId.Log).HasEventsFor(_noteId);
    }

    [Fact] void should_publish_one_event() => _outbox.Count.ShouldEqual(1);
    [Fact] void should_publish_it_as_note_on_context_given() => _published.ShouldNotBeNull();
    [Fact] void should_publish_it_for_the_note() => _outbox.Single().Context.EventSourceId.Value.ShouldEqual(_noteId.Value.ToString());
    [Fact] void should_name_the_namespace_as_tenant() => _published.TenantId.ShouldEqual(new TenantId(Tenant.Value));
    [Fact] void should_carry_the_application() => _published.Application.ShouldEqual(_given.Application);
    [Fact] void should_carry_the_giver() => _published.Giver.ShouldEqual(_given.Giver);
    [Fact] void should_carry_the_location() => _published.Context.Location.ShouldEqual(_given.Context.Location);
    [Fact] void should_carry_the_active_slices() => _published.Context.ActiveSlices.ShouldContainOnly(_given.Context.ActiveSlices);
    [Fact] void should_carry_the_screen() => _published.Context.Screen.ShouldEqual(_given.Context.Screen);
    [Fact] void should_carry_the_route() => _published.Context.Route.ShouldEqual(_given.Context.Route);
    [Fact] void should_carry_the_dialog() => _published.Context.Dialog.ShouldEqual(_given.Context.Dialog);
    [Fact] void should_carry_the_model_references() => _published.Context.ModelReferences["event-model"].ShouldEqual("billing");
    [Fact] void should_carry_the_correlation_id_of_the_context() => _published.Context.CorrelationId.ShouldEqual(_given.Context.CorrelationId);
    [Fact] void should_carry_the_kind() => _published.Kind.ShouldEqual(_given.Kind);
    [Fact] void should_carry_the_text() => _published.Text.ShouldEqual(_given.Text);
    [Fact] void should_keep_when_it_happened() => _outbox.Single().Context.Occurred.ShouldEqual(Occurred);
    [Fact] void should_keep_the_correlation_id() => _outbox.Single().Context.CorrelationId.ShouldEqual(_correlationId);
    [Fact] void should_keep_the_compliance_subject() => _outbox.Single().Context.Subject.ShouldEqual(NoteGivers.For(Givers.UserId));
    [Fact] void should_not_append_anything_to_the_event_log() => _eventLogHasEvents.ShouldBeFalse();
}
#endif
