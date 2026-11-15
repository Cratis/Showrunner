// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

/// <summary>
/// A published application, the default, publishes the note and nothing about its kind: this is how every note was published
/// before the kind could be named.
/// </summary>
public class from_a_published_application : an_outbox_publisher
{
    protected override NotesOptions Options => new() { ApplicationKind = ApplicationKind.Published };

    async Task Because()
    {
        var context = ContextFor();
        await _publisher.GaveNoteOnContext(NoteEvents.Given(), context, DeliveryOf(context));
        await ReadOutbox();
    }

    [Fact] void should_publish_only_the_note() => _outbox.Single().Content.ShouldBeOfExactType<NoteOnContextGiven>();
}
#endif
