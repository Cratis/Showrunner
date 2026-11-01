// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

/// <summary>
/// The forwarder can put the same event into the inbox twice, and redelivery repeats the inbox event; the note is recorded
/// once.
/// </summary>
public class and_the_same_note_arrives_twice : an_app_event_translator
{
    async Task Because()
    {
        await _translator.Translate(Given(Tenant.Value), InboxContext(7), Application);
        await _translator.Translate(Given(Tenant.Value), InboxContext(8), Application);
        await _translator.Translate(Given(Tenant.Value), InboxContext(7), Application);
        await ReadRecorded();
    }

    [Fact] void should_record_it_once() => _recorded.Count.ShouldEqual(1);
}
#endif
