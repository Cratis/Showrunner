// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Publishing.for_ResponsePublisher.given;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Contracts;

namespace Cratis.Showrunner.Notes.Collecting.Publishing.for_ResponsePublisher.when_publishing;

public class a_note_from_a_named_application : a_response_publisher
{
    NamedTag[] _tags;

    async Task Because()
    {
        using (EventTags.Scope(AudienceTags.From, "ledger-app"))
        {
            await _eventStore.EventLog.Append(_noteId, ReceivedNotes.OnContext);
        }

        // The operator's answer is appended without the tag; the sender is found on the note's history.
        await _eventStore.EventLog.Append(_noteId, new OperatorAcknowledgedNote());
        var context = ContextFor(1);
        await _publisher.Acknowledged(new OperatorAcknowledgedNote(), context, DeliveryOf(context));
        await ReadOutbox();
        _tags = [.. _outbox.Single().Context.NamedTags];
    }

    [Fact] void should_address_the_answer_to_the_sending_application() => _tags.ShouldContain(new NamedTag(WellKnownTags.Audience, "ledger-app"));
}
#endif
