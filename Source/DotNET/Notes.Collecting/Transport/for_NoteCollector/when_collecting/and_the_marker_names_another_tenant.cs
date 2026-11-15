// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

/// <summary>
/// The marker is held to the same rule as the note: it must name the publication's tenant, or the whole batch is rejected.
/// </summary>
public class and_the_marker_names_another_tenant : a_note_collector
{
    CollectOutcome _outcome;

    async Task Because() => _outcome = await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(new NoteGivenOnStagedApplication("another-tenant"), 0)]);

    [Fact] void should_reject_the_batch() => _outcome.ShouldEqual(CollectOutcome.TenantMismatch);
    [Fact] async Task should_record_nothing() => (await RecordedAnywhere()).Count.ShouldEqual(0);
}
#endif
