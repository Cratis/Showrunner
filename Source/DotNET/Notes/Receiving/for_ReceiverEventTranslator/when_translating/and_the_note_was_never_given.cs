// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.given;

namespace Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.when_translating;

public class and_the_note_was_never_given : a_receiver_event_translator
{
    async Task Because()
    {
        await _translator.Translate(new NoteAcknowledged(Tenant.Value), InboxContext(), Collecting);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.Count.ShouldEqual(0);
    [Fact] void should_warn() => _logger.Messages.ShouldContain(_ => _.Name == nameof(ReceiverEventTranslatorLogging.UnknownNote));
}
#endif
