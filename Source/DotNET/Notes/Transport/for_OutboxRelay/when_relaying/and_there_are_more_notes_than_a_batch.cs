// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_OutboxRelay.when_relaying;

public class and_there_are_more_notes_than_a_batch : an_outbox_relay
{
    async Task Because()
    {
        _options.RelayBatchSize = 2;
        for (var i = 0; i < 5; i++)
        {
            await Publish(NoteId.New());
        }

        await _relay.Relay(_eventStore);
    }

    [Fact] void should_send_them_in_batches() => _collector.Sent.Select(_ => _.Notes.Count).ShouldContainOnly(2, 2, 1);
}
#endif
