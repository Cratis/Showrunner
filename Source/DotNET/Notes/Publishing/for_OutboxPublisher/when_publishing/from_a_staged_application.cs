// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;
using Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

/// <summary>
/// A staged application says so after each note it publishes, once, under the note's id and for the tenant, and says nothing
/// about a withdrawal.
/// </summary>
public class from_a_staged_application : an_outbox_publisher
{
    protected override NotesOptions Options => new() { ApplicationKind = ApplicationKind.Staged };

    async Task Because()
    {
        var context = ContextFor();
        var delivery = DeliveryOf(context);
        await _publisher.GaveNoteOnContext(NoteEvents.Given(), context, delivery);
        await _publisher.GaveNoteOnContext(NoteEvents.Given(), context, delivery);
        await _publisher.GaveNoteOnSlices(new(NoteEvents.Application, Givers.Giver, NoteEvents.Context, [NoteEvents.Location], NoteKind.Suggestion, "Merge them."), ContextFor(1), DeliveryOf(ContextFor(1)));
        await _publisher.GaveNoteOnScreen(new(NoteEvents.Application, Givers.Giver, NoteEvents.Context, NoteKind.Question, "Why?"), ContextFor(2), DeliveryOf(ContextFor(2)));
        await _publisher.GaveNoteOnComponent(new(NoteEvents.Application, Givers.Giver, NoteEvents.Context, NoteEvents.Component, NoteKind.Praise, "Nice."), ContextFor(3), DeliveryOf(ContextFor(3)));
        await _publisher.SuggestedPropertyChange(NoteEvents.Suggested(), ContextFor(4), DeliveryOf(ContextFor(4)));
        await _publisher.WithdrewNote(new GiverWithdrewNote(), ContextFor(5), DeliveryOf(ContextFor(5)));
        await ReadOutbox();
    }

    [Fact] void should_follow_each_note_with_the_marker_once() => _outbox.Count(_ => _.Content is NoteGivenOnStagedApplication).ShouldEqual(5);
    [Fact] void should_publish_the_marker_right_after_the_note() => _outbox[1].Content.ShouldBeOfExactType<NoteGivenOnStagedApplication>();
    [Fact] void should_publish_the_marker_under_the_note() => _outbox[1].Context.EventSourceId.Value.ShouldEqual(_noteId.Value.ToString());
    [Fact] void should_name_the_tenant_in_the_marker() => ((NoteGivenOnStagedApplication)_outbox[1].Content).TenantId.ShouldEqual(new TenantId(Tenant.Value));
    [Fact] void should_keep_the_compliance_subject_on_the_marker() => _outbox[1].Context.Subject.ShouldEqual(Identity.NoteGivers.For(Givers.UserId));
    [Fact] void should_not_mark_a_withdrawal() => _outbox[^1].Content.ShouldBeOfExactType<NoteWithdrawn>();
    [Fact] void should_publish_nothing_else() => _outbox.Count.ShouldEqual(11);
}
#endif
