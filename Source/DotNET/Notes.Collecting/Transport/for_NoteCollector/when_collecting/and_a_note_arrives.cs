// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

/// <summary>
/// The note is recorded as the private fact the same-kernel transport records, in the namespace the publication names.
/// </summary>
public class and_a_note_arrives : a_note_collector
{
    CollectOutcome _outcome;
    AppendedEvent _recorded;

    async Task Because()
    {
        _outcome = await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 4)]);
        _recorded = (await Recorded()).Single();
    }

    static string Json(object content) => System.Text.Json.JsonSerializer.Serialize(content, content.GetType(), Cratis.Showrunner.Notes.Contracts.Transport.NotesTransportJson.Options);

    [Fact] void should_store_it() => _outcome.ShouldEqual(CollectOutcome.Stored);
    [Fact] void should_record_the_same_fact_as_the_translator_does() => Json(_recorded.Content).ShouldEqual(Json(ReceivedNotes.OnContext));
    [Fact] void should_say_which_application_it_came_from() => ((NoteOnContextReceived)_recorded.Content).AppStore.ShouldEqual(new AppStoreName("Ledger"));
    [Fact] void should_keep_when_it_happened() => _recorded.Context.Occurred.ShouldEqual(Occurred);
    [Fact] void should_keep_the_correlation_id() => _recorded.Context.CorrelationId.Value.ShouldEqual(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));
    [Fact] void should_keep_the_compliance_subject() => _recorded.Context.Subject.ShouldEqual(new Subject("notes-giver-abc"));
}
#endif
