// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

/// <summary>
/// The marker of a staged session is recorded on the note, once, however often the inbox delivers it.
/// </summary>
public class and_a_note_was_given_in_a_staged_session : an_app_event_translator
{
    async Task Because()
    {
        await _translator.Translate(Given(Tenant.Value), InboxContext(0), Application);
        await _translator.Translate(new NoteGivenOnStagedApplication(Tenant.Value), InboxContext(1), Application);
        await _translator.Translate(new NoteGivenOnStagedApplication(Tenant.Value), InboxContext(1), Application);
        await ReadRecorded();
    }

    [Fact] void should_record_the_marker_once() => _recorded.Count(_ => _.Content is NoteOnStagedApplicationReceived).ShouldEqual(1);
    [Fact] void should_record_it_on_the_note() => _recorded[^1].Content.ShouldBeOfExactType<NoteOnStagedApplicationReceived>();
}
#endif
