// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Cratis.Showrunner.Notes.Contracts.Transport;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_asking_for_the_access;

/// <summary>
/// A caller that names the version it has is answered nothing while it is current, and the list again once the desk decided
/// something, even though the list may look alike.
/// </summary>
public class and_the_caller_has_the_current_version : a_note_collector
{
    FeedbackAccessGrants _first;
    FeedbackAccessGrants _current;
    FeedbackAccessGrants _afterADecision;

    async Task Because()
    {
        await DeskDecisions.Grant(_tenantStore.EventLog, DeskDecisions.User("ada"), 0);
        (_, _first) = await _collector.Access(PublicationName, _publication, null);
        (_, _current) = await _collector.Access(PublicationName, _publication, _first.Version);
        await DeskDecisions.Revoke(_tenantStore.EventLog, DeskDecisions.User("grace"), 1);
        (_, _afterADecision) = await _collector.Access(PublicationName, _publication, _first.Version);
    }

    [Fact] void should_answer_nothing_while_current() => _current.ShouldBeNull();
    [Fact] void should_answer_the_list_after_a_decision() => _afterADecision.Version.ShouldBeGreaterThan(_first.Version);
}
#endif
