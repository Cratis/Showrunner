// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Publishing.for_ResponsePublisher.given;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;

namespace Cratis.Showrunner.Notes.Collecting.Publishing.for_ResponsePublisher.when_publishing;

public class each_answer : a_response_publisher
{
    NoteAcknowledged _acknowledged;
    NoteAddressed _addressed;
    NoteDeclined _declined;

    async Task Because()
    {
        await _publisher.Acknowledged(new OperatorAcknowledgedNote(), ContextFor(1), DeliveryOf(ContextFor(1)));
        await _publisher.Addressed(new OperatorAddressedNote("2.0.0"), ContextFor(2), DeliveryOf(ContextFor(2)));
        await _publisher.Declined(new OperatorDeclinedNote("Out of scope."), ContextFor(3), DeliveryOf(ContextFor(3)));
        await ReadOutbox();
        _acknowledged = _outbox.Select(_ => _.Content).OfType<NoteAcknowledged>().Single();
        _addressed = _outbox.Select(_ => _.Content).OfType<NoteAddressed>().Single();
        _declined = _outbox.Select(_ => _.Content).OfType<NoteDeclined>().Single();
    }

    [Fact] void should_publish_three_events() => _outbox.Count.ShouldEqual(3);
    [Fact] void should_publish_them_for_the_note() => _outbox.ShouldEachConformTo(_ => _.Context.EventSourceId.Value == _noteId.Value.ToString());
    [Fact] void should_name_the_namespace_as_tenant_on_the_acknowledgement() => _acknowledged.TenantId.ShouldEqual(new TenantId(Tenant.Value));
    [Fact] void should_name_the_version() => _addressed.AddressedInVersion.ShouldEqual(new AddressedInVersion("2.0.0"));
    [Fact] void should_name_the_tenant_on_the_resolution() => _addressed.TenantId.ShouldEqual(new TenantId(Tenant.Value));
    [Fact] void should_give_the_reason() => _declined.Reason.ShouldEqual(new DeclineReason("Out of scope."));
    [Fact] void should_keep_when_it_happened() => _outbox.ShouldEachConformTo(_ => _.Context.Occurred == Occurred);
}
#endif
