// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.given;

namespace Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.when_translating;

/// <summary>
/// The forwarder can put the same event into the inbox twice; the answer is recorded once.
/// </summary>
public class and_the_same_event_arrives_twice : a_receiver_event_translator
{
    async Task Because()
    {
        await Given();
        await _translator.Translate(new NoteAddressed(Tenant.Value, "2.0.0"), InboxContext(7), Collecting);
        await _translator.Translate(new NoteAddressed(Tenant.Value, "2.0.0"), InboxContext(8), Collecting);
        await ReadRecorded();
    }

    [Fact] void should_record_it_once() => _recorded.Count(_ => _.Content is ReceiverAddressedNote).ShouldEqual(1);
}
#endif
