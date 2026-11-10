// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

public class and_the_note_cannot_be_read : a_note_collector
{
    CollectOutcome _unreadableContent;
    CollectOutcome _notANote;

    async Task Because()
    {
        _unreadableContent = await _collector.Collect(
            PublicationName,
            _publication,
            string.Empty,
            [Relayed(Given(Tenant.Value), 4) with { Content = JsonElement.Parse("{\"tenantId\":[1,2]}") }]);
        _notANote = await _collector.Collect(
            PublicationName,
            _publication,
            string.Empty,
            [Relayed(Given(Tenant.Value), 5) with { EventSourceId = "not-a-guid" }]);
    }

    [Fact] void should_reject_unreadable_content() => _unreadableContent.ShouldEqual(CollectOutcome.Invalid);
    [Fact] void should_reject_a_note_without_a_note_id() => _notANote.ShouldEqual(CollectOutcome.Invalid);
    [Fact] async Task should_record_nothing() => (await RecordedAnywhere()).Count.ShouldEqual(0);
}
#endif
