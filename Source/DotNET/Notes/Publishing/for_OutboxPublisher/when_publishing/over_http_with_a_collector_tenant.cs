// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.given;

namespace Cratis.Showrunner.Notes.Publishing.for_OutboxPublisher.when_publishing;

/// <summary>
/// The collector rejects a note whose tenant is not the publication's, so the application names the tenant the collector
/// knows instead of its own namespace.
/// </summary>
public class over_http_with_a_collector_tenant : an_outbox_publisher
{
    protected override NotesOptions Options => new() { Transport = NotesTransport.Http, CollectorTenantId = "acme-corp" };

    async Task Because()
    {
        var context = ContextFor();
        await _publisher.GaveNoteOnContext(NoteEvents.Given(), context, DeliveryOf(context));
        await ReadOutbox();
    }

    [Fact] void should_name_the_collectors_tenant() => ((NoteOnContextGiven)_outbox.Single().Content).TenantId.Value.ShouldEqual("acme-corp");
}
#endif
