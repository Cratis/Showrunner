// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

public class and_a_note_is_withdrawn : an_app_event_translator
{
    async Task Because()
    {
        await _translator.Translate(Given(Tenant.Value), InboxContext(0), Application);
        await _translator.Translate(new NoteWithdrawn(Tenant.Value), InboxContext(1), Application);
        await ReadRecorded();
    }

    [Fact] void should_record_the_withdrawal() => _recorded.Count(_ => _.Content is NoteWithdrawalReceived).ShouldEqual(1);
}
#endif
