// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

public class and_only_one_note_of_the_batch_names_another_tenant : a_note_collector
{
    CollectOutcome _outcome;
    ulong? _position;

    async Task Because()
    {
        _outcome = await _collector.Collect(
            PublicationName,
            _publication,
            string.Empty,
            [Relayed(Given(Tenant.Value), 4), Relayed(Given("another-tenant"), 5, NoteId.New())]);
        _position = (await _collector.Position(PublicationName, _publication, string.Empty)).Position;
    }

    [Fact] void should_reject_the_batch() => _outcome.ShouldEqual(CollectOutcome.TenantMismatch);
    [Fact] async Task should_store_none_of_it() => (await RecordedAnywhere()).Count.ShouldEqual(0);
    [Fact] void should_not_move_the_position() => _position.ShouldBeNull();
}
#endif
