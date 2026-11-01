// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

public class and_the_event_is_not_one_from_an_application : an_app_event_translator
{
    async Task Because()
    {
        await _translator.Translate(new NoteAcknowledged(Tenant.Value), InboxContext(), Application);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.Count.ShouldEqual(0);
    [Fact] void should_warn() => _logger.Messages.ShouldContain(_ => _.Name == nameof(AppEventTranslatorLogging.NotAnEventFromAnApplication));
}
#endif
