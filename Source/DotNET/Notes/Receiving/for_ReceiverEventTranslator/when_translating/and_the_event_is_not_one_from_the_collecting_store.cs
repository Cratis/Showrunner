// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.given;

namespace Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.when_translating;

public class and_the_event_is_not_one_from_the_collecting_store : a_receiver_event_translator
{
    async Task Because()
    {
        await Given();
        await _translator.Translate(new NoteWithdrawn(Tenant.Value), InboxContext(), Collecting);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing_new() => _recorded.Count.ShouldEqual(1);
    [Fact] void should_warn() => _logger.Messages.ShouldContain(_ => _.Name == nameof(ReceiverEventTranslatorLogging.NotAnEventFromTheReceiver));
}
#endif
