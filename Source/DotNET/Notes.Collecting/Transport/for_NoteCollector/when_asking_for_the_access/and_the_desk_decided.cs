// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.Contracts.Transport;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_asking_for_the_access;

/// <summary>
/// The list is what the desk's latest decision about each user leaves granted: a revocation takes a user out, granting again
/// puts them back, a decision made earlier than the one held does not change it even when it arrives later, and at the same
/// time the revocation wins.
/// </summary>
public class and_the_desk_decided : a_note_collector
{
    static readonly FeedbackGiverIdentity Granted = DeskDecisions.User("granted");
    static readonly FeedbackGiverIdentity Revoked = DeskDecisions.User("revoked");
    static readonly FeedbackGiverIdentity Regranted = DeskDecisions.User("regranted");
    static readonly FeedbackGiverIdentity Late = DeskDecisions.User("late-grant");
    static readonly FeedbackGiverIdentity Tied = DeskDecisions.User("tied");
    static readonly FeedbackGiverIdentity Never = DeskDecisions.User("never");

    CollectOutcome _outcome;
    FeedbackAccessGrants _grants;

    async Task Because()
    {
        var log = _tenantStore.EventLog;
        await DeskDecisions.Grant(log, Granted, 0);
        await DeskDecisions.Grant(log, Revoked, 1);
        await DeskDecisions.Revoke(log, Revoked, 2);
        await DeskDecisions.Grant(log, Regranted, 3);
        await DeskDecisions.Revoke(log, Regranted, 4);
        await DeskDecisions.Grant(log, Regranted, 5);
        await DeskDecisions.Revoke(log, Late, 10);
        await DeskDecisions.Grant(log, Late, 6);
        await DeskDecisions.Grant(log, Tied, 7);
        await DeskDecisions.Revoke(log, Tied, 7);
        (_outcome, _grants) = await _collector.Access(PublicationName, _publication, null);
    }

    [Fact] void should_store() => _outcome.ShouldEqual(CollectOutcome.Stored);
    [Fact] void should_name_who_is_granted() => _grants.Givers.ShouldContainOnly(FeedbackGiverId.For(Granted).Value, FeedbackGiverId.For(Regranted).Value);
    [Fact] void should_not_name_a_revoked_user() => _grants.Givers.ShouldNotContain(FeedbackGiverId.For(Revoked).Value);
    [Fact] void should_not_let_a_late_arriving_earlier_grant_win() => _grants.Givers.ShouldNotContain(FeedbackGiverId.For(Late).Value);
    [Fact] void should_let_the_revocation_win_a_tie() => _grants.Givers.ShouldNotContain(FeedbackGiverId.For(Tied).Value);
    [Fact] void should_not_name_a_user_the_desk_never_decided_about() => _grants.Givers.ShouldNotContain(FeedbackGiverId.For(Never).Value);
    [Fact] void should_version_the_list_after_the_last_decision() => _grants.Version.ShouldEqual(10ul);
}
#endif
