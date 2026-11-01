// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.given;

namespace Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.when_translating;

public class and_the_event_names_no_tenant : a_receiver_event_translator
{
    async Task Because()
    {
        await Given();
        await _translator.Translate(new NoteAcknowledged(TenantId.NotSet), InboxContext(), Collecting);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.Count(_ => _.Content is ReceiverAcknowledgedNote).ShouldEqual(0);
    [Fact] void should_warn() => _logger.Messages.ShouldContain(_ => _.Name == nameof(ReceiverEventTranslatorLogging.NoTenant));
}
#endif
