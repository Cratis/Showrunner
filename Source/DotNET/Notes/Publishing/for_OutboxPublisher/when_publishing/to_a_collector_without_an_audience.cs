// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

public class to_a_collector_without_an_audience : an_outbox_publisher
{
    protected override NotesOptions Options => new() { CollectorAudience = null };

    NamedTag[] _tags;

    async Task Because()
    {
        var context = ContextFor();
        await _publisher.GaveNoteOnContext(NoteEvents.Given(), context, DeliveryOf(context));
        await ReadOutbox();
        _tags = [.. _outbox.Single().Context.NamedTags];
    }

    [Fact] void should_not_address_the_note() => _tags.ShouldBeEmpty();
}
#endif
