// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Contracts;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

public class from_an_application_that_names_itself : an_outbox_publisher
{
    protected override NotesOptions Options => new() { Audience = "ledger-app", CollectorAudience = "studio" };

    NamedTag[] _tags;

    async Task Because()
    {
        var context = ContextFor();
        await _publisher.GaveNoteOnContext(NoteEvents.Given(), context, DeliveryOf(context));
        await ReadOutbox();
        _tags = [.. _outbox.Single().Context.NamedTags];
    }

    [Fact] void should_address_the_note_to_the_collector() => _tags.ShouldContain(new NamedTag(WellKnownTags.Audience, "studio"));
    [Fact] void should_name_the_sending_application() => _tags.ShouldContain(new NamedTag(new TagName(AudienceTags.From), "ledger-app"));
}
#endif
