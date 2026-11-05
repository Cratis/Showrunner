// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Publishing.for_ResponsePublisher.given;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;

namespace Cratis.Showrunner.Notes.Collecting.Publishing.for_ResponsePublisher.when_publishing;

public class a_note_from_an_application_that_does_not_name_itself : a_response_publisher
{
    NamedTag[] _tags;

    async Task Because()
    {
        await _eventStore.EventLog.Append(_noteId, ReceivedNotes.OnContext);
        var context = ContextFor(1);
        await _publisher.Acknowledged(new OperatorAcknowledgedNote(), context, DeliveryOf(context));
        await ReadOutbox();
        _tags = [.. _outbox.Single().Context.NamedTags];
    }

    [Fact] void should_leave_the_answer_unaddressed() => _tags.ShouldBeEmpty();
}
#endif
