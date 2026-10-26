// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

/// <summary>
/// A lapse of a grant that was already revoked and replaced changes nothing; the lapse of the current grant ends it.
/// </summary>
public class when_grants_lapse : a_read_model<IssueConversation>
{
    static readonly ValidUntil _earlier = new(Start.AddMinutes(30));
    static readonly ValidUntil _later = new(Start.AddHours(2));
    IssueAccess _afterStaleLapse;
    int _entriesAfterStaleLapse;
    IssueConversation _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Subject),
            new ReporterGrantedAccess(_earlier),
            new ReporterRevokedAccess(),
            new ReporterGrantedAccess(_later),
            new AccessGrantLapsed(_earlier));
        _afterStaleLapse = _scenario.Instance!.Access;
        _entriesAfterStaleLapse = _scenario.Instance!.Entries.Count();
        await Happened(new AccessGrantLapsed(_later));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_stay_granted_after_a_stale_lapse() => _afterStaleLapse.State.ShouldEqual(AccessState.Granted);
    [Fact] void should_not_record_a_stale_lapse() => _entriesAfterStaleLapse.ShouldEqual(4);
    [Fact] void should_be_lapsed_after_the_current_grant_lapses() => _issue.Access.State.ShouldEqual(AccessState.Lapsed);
    [Fact] void should_keep_the_end_of_the_grant() => _issue.Access.ValidUntil.ShouldEqual(_later);
    [Fact] void should_record_the_lapse_as_the_system() => (_issue.Entries.Last().Kind, _issue.Entries.Last().AuthorKind).ShouldEqual((ConversationEntryKind.AccessLapsed, ConversationAuthorKind.System));
}
#endif
