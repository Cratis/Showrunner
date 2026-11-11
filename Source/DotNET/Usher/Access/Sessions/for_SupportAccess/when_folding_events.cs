// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Issues;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportAccess;

public class when_folding_events : Specification
{
    static readonly DateTimeOffset _now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    static readonly DateTimeOffset _earlier = _now.AddHours(1);
    static readonly DateTimeOffset _later = _now.AddHours(2);
    readonly Dictionary<string, SupportAccess> _folded = new(StringComparer.Ordinal);

    static SupportAccess Fold(params object[] events) => SupportAccess.From(events.Select((@event, index) => Appended(@event, (ulong)index + 1)));

    static AppendedEvent Appended(object @event, ulong sequenceNumber) => new(
        EventContext.From(
            EventStoreName.NotSet,
            EventStoreNamespaceName.NotSet,
            EventType.Unknown,
            EventSourceType.Default,
            IssueId.New(),
            EventStreamType.All,
            EventStreamId.Default,
            sequenceNumber,
            CorrelationId.New(),
            _now),
        @event);

    void Because()
    {
        var described = IssueEvents.Described(Reporters.Identity);
        var hijack = IssueEvents.Described(Reporters.SomeoneElsesIdentity);
        var signedIn = new SupportSignedInAsReporter(Reporters.SessionId, IssueEvents.Support, IssueEvents.SupportPerson);
        _folded["described_then_flagged"] = Fold(described, IssueEvents.Flagged(Reporters.SomeoneElsesIdentity));
        _folded["flagged_then_described"] = Fold(IssueEvents.Flagged(Reporters.Identity), hijack);
        _folded["shared"] = Fold(described, new ReporterSharedIdentity(Reporters.Reporter));
        _folded["withdrawn_then_granted"] = Fold(described, new ReporterWithdrewIssue(), new ReporterGrantedAccess(_later));
        _folded["granted_then_withdrawn"] = Fold(described, new ReporterGrantedAccess(_later), new ReporterWithdrewIssue());
        _folded["revoked"] = Fold(described, new ReporterGrantedAccess(_later), new ReporterRevokedAccess());
        _folded["stale_lapse"] = Fold(described, new ReporterGrantedAccess(_earlier), new ReporterGrantedAccess(_later), new AccessGrantLapsed(_earlier));
        _folded["current_lapse"] = Fold(described, new ReporterGrantedAccess(_later), new AccessGrantLapsed(_later));
        _folded["signed_in_twice"] = Fold(described, signedIn, signedIn with { Author = IssueEvents.Agent });
        _folded["ended_unknown"] = Fold(described, new SupportSessionEnded(Reporters.SessionId));
        _folded["ended_then_signed_in"] = Fold(described, signedIn, new SupportSessionEnded(Reporters.SessionId), signedIn with { Author = IssueEvents.Agent });
        _folded["permitted"] = Fold(described, new SupportSessionPermitted(Reporters.SessionId));
        _folded["signed_in"] = Fold(described, signedIn);
        _folded["unrelated"] = Fold(described, "not an issue event", new object());
        _folded["live_grant"] = Fold(described, new ReporterGrantedAccess(_now));
    }

    [Fact] void should_keep_the_first_reporter_over_a_later_flag() => _folded["described_then_flagged"].ReportedBy.ShouldEqual(Reporters.Identity);
    [Fact] void should_keep_the_first_flagger_over_a_later_description() => _folded["flagged_then_described"].ReportedBy.ShouldEqual(Reporters.Identity);
    [Fact] void should_know_the_issue_is_reported() => _folded["shared"].IsReported.ShouldBeTrue();
    [Fact] void should_keep_the_shared_identity() => _folded["shared"].SharedIdentity.ShouldEqual(Reporters.Reporter);
    [Fact] void should_ignore_a_grant_after_withdrawal() => _folded["withdrawn_then_granted"].Granted.ShouldBeFalse();
    [Fact] void should_keep_the_issue_withdrawn_after_a_later_grant() => _folded["withdrawn_then_granted"].Withdrawn.ShouldBeTrue();
    [Fact] void should_not_record_the_end_of_a_grant_after_withdrawal() => _folded["withdrawn_then_granted"].ValidUntil.ShouldEqual(Unset.ValidUntil);
    [Fact] void should_end_a_grant_by_withdrawing() => _folded["granted_then_withdrawn"].Granted.ShouldBeFalse();
    [Fact] void should_end_a_grant_by_revoking() => _folded["revoked"].Granted.ShouldBeFalse();
    [Fact] void should_not_withdraw_by_revoking() => _folded["revoked"].Withdrawn.ShouldBeFalse();
    [Fact] void should_keep_a_later_grant_when_an_earlier_one_lapses() => _folded["stale_lapse"].Granted.ShouldBeTrue();
    [Fact] void should_keep_the_end_of_the_later_grant() => _folded["stale_lapse"].ValidUntil.ShouldEqual((ValidUntil)_later);
    [Fact] void should_end_the_current_grant_when_it_lapses() => _folded["current_lapse"].Granted.ShouldBeFalse();
    [Fact] void should_not_reattribute_a_started_session() => _folded["signed_in_twice"].Sessions[Reporters.SessionId].Author.ShouldEqual(IssueEvents.Support);
    [Fact] void should_keep_the_first_start_of_a_session() => _folded["signed_in_twice"].Sessions[Reporters.SessionId].StartedAt.Value.ShouldEqual(2UL);
    [Fact] void should_not_create_a_session_by_ending_an_unknown_one() => _folded["ended_unknown"].Sessions.Count.ShouldEqual(0);
    [Fact] void should_not_know_an_unknown_session_ended() => _folded["ended_unknown"].HasEnded(Reporters.SessionId).ShouldBeFalse();
    [Fact] void should_keep_an_ended_session_ended() => _folded["ended_then_signed_in"].HasEnded(Reporters.SessionId).ShouldBeTrue();
    [Fact] void should_not_restart_an_ended_session() => _folded["ended_then_signed_in"].Sessions[Reporters.SessionId].Author.ShouldEqual(IssueEvents.Support);
    [Fact] void should_have_no_open_session_once_it_ended() => _folded["ended_then_signed_in"].OpenSessions.Count().ShouldEqual(0);
    [Fact] void should_have_an_open_session_once_it_started() => _folded["signed_in"].OpenSessions.Single().ShouldEqual(Reporters.SessionId);
    [Fact] void should_remember_a_permitted_session() => _folded["permitted"].WasPermitted(Reporters.SessionId).ShouldBeTrue();
    [Fact] void should_not_start_a_session_by_permitting_it() => _folded["permitted"].HasStarted(Reporters.SessionId).ShouldBeFalse();
    [Fact] void should_know_a_started_session_started() => _folded["signed_in"].HasStarted(Reporters.SessionId).ShouldBeTrue();
    [Fact] void should_not_permit_a_session_by_starting_it() => _folded["signed_in"].WasPermitted(Reporters.SessionId).ShouldBeFalse();
    [Fact] void should_ignore_unrelated_events() => _folded["unrelated"].ReportedBy.ShouldEqual(Reporters.Identity);
    [Fact] void should_advance_to_the_last_event_even_when_ignored() => _folded["unrelated"].LastSequenceNumber.Value.ShouldEqual(3UL);
    [Fact] void should_advance_to_the_last_event_of_a_fold() => _folded["signed_in"].LastSequenceNumber.Value.ShouldEqual(2UL);
    [Fact] void should_not_have_a_live_grant_at_its_end() => _folded["live_grant"].HasLiveGrant(_now).ShouldBeFalse();
    [Fact] void should_have_a_live_grant_just_before_its_end() => _folded["live_grant"].HasLiveGrant(_now.AddTicks(-1)).ShouldBeTrue();
}
#endif
