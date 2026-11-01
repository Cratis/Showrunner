// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

/// <summary>
/// Chronicle delivers at least once, so a delivery that is repeated publishes nothing the second time.
/// </summary>
public class the_same_fact_twice : an_outbox_publisher
{
    async Task Because()
    {
        var context = ContextFor(3);
        var delivery = DeliveryOf(context);
        await _publisher.GaveNoteOnContext(NoteEvents.Given(), context, delivery);
        await _publisher.GaveNoteOnContext(NoteEvents.Given(), context, delivery);
        await ReadOutbox();
    }

    [Fact] void should_publish_it_once() => _outbox.Count.ShouldEqual(1);
}
#endif
