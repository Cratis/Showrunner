// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;
using Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

public class the_other_kinds_of_note : an_outbox_publisher
{
    async Task Because()
    {
        var context = ContextFor();
        await _publisher.GaveNoteOnSlices(new(NoteEvents.Application, Givers.Giver, NoteEvents.Context, [NoteEvents.Location], NoteKind.Suggestion, "Merge them."), context, DeliveryOf(context));
        await _publisher.GaveNoteOnScreen(new(NoteEvents.Application, Givers.Giver, NoteEvents.Context, NoteKind.Question, "Why?"), ContextFor(1), DeliveryOf(ContextFor(1)));
        await _publisher.GaveNoteOnComponent(new(NoteEvents.Application, Givers.Giver, NoteEvents.Context, NoteEvents.Component, NoteKind.Praise, "Nice."), ContextFor(2), DeliveryOf(ContextFor(2)));
        await _publisher.SuggestedPropertyChange(NoteEvents.Suggested(), ContextFor(3), DeliveryOf(ContextFor(3)));
        await _publisher.WithdrewNote(new GiverWithdrewNote(), ContextFor(4), DeliveryOf(ContextFor(4)));
        await ReadOutbox();
    }

    [Fact] void should_publish_each_as_its_own_event() => _outbox.Select(_ => _.Content.GetType()).ShouldContainOnly(
        typeof(NoteOnSlicesGiven),
        typeof(NoteOnScreenGiven),
        typeof(NoteOnComponentGiven),
        typeof(ComponentPropertyChangeSuggested),
        typeof(NoteWithdrawn));

    [Fact] void should_name_the_tenant_on_each() => _outbox.Select(_ => _.Content).ShouldEachConformTo(content => content.GetType().GetProperty("TenantId")!.GetValue(content)!.Equals(new TenantId(Tenant.Value)));
}
#endif
