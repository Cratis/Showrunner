// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Cratis.Showrunner.Notes.Contracts.Transport;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_asking_for_the_access;

/// <summary>
/// The desk decided nothing in the tenant: the list is complete and empty at version zero, not an error, and a caller that
/// already has version zero is told nothing changed.
/// </summary>
public class and_the_desk_decided_nothing : a_note_collector
{
    FeedbackAccessGrants _grants;
    FeedbackAccessGrants _unchanged;
    CollectOutcome _outcome;

    async Task Because()
    {
        (_outcome, _grants) = await _collector.Access(PublicationName, _publication, null);
        (_, _unchanged) = await _collector.Access(PublicationName, _publication, 0);
    }

    [Fact] void should_store() => _outcome.ShouldEqual(CollectOutcome.Stored);
    [Fact] void should_serve_an_empty_list() => _grants.Givers.Count.ShouldEqual(0);
    [Fact] void should_be_at_version_zero() => _grants.Version.ShouldEqual(0ul);
    [Fact] void should_say_nothing_changed_to_a_caller_that_has_it() => _unchanged.ShouldBeNull();
}
#endif
