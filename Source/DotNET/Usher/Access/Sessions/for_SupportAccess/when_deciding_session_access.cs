// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportAccess;

public class when_deciding_session_access : Specification
{
    static readonly DateTimeOffset _now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    readonly Dictionary<string, SupportSessionRefusal?> _refusals = new(StringComparer.Ordinal);

    void Because()
    {
        var request = new SupportSessionRequest(IssueId.New(), Reporters.SessionId, Reporters.Subject, IssueEvents.SupportPerson);
        var live = SupportAccess.None with { ReportedBy = Reporters.Identity, SharedIdentity = Reporters.Reporter, Granted = true, ValidUntil = _now.AddHours(1) };
        var ended = live with { Sessions = live.Sessions.Add(Reporters.SessionId, new(IssueEvents.Support, EventSequenceNumber.First, true)) };
        _refusals["missing"] = SupportAccess.None.RefusalFor(request, _now);
        _refusals["subject"] = live.RefusalFor(request with { Subject = Reporters.SomeoneElse }, _now);
        _refusals["self"] = live.RefusalFor(request with { Actor = Reporters.Identity }, _now);
        _refusals["provider"] = live.RefusalFor(request with { Actor = Reporters.SameSubjectAtAnotherProvider }, _now);
        _refusals["ended"] = ended.RefusalFor(request, _now);
        _refusals["withdrawn"] = (live with { Withdrawn = true }).RefusalFor(request, _now);
        _refusals["revoked"] = (live with { Granted = false }).RefusalFor(request, _now);
        _refusals["expired"] = (live with { ValidUntil = _now.AddTicks(-1) }).RefusalFor(request, _now);
        _refusals["boundary"] = (live with { ValidUntil = _now }).RefusalFor(request, _now);
        _refusals["unshared"] = (live with { SharedIdentity = null }).RefusalFor(request, _now);
        _refusals["wrong_identity"] = (live with { SharedIdentity = Reporters.Reporter with { IdentityProvider = Reporters.AnotherIdentityProvider } }).RefusalFor(request, _now);
        _refusals["live"] = live.RefusalFor(request, _now);
        _refusals["subject_before_self"] = live.RefusalFor(request with { Subject = Reporters.SomeoneElse, Actor = Reporters.Identity }, _now);
        _refusals["self_before_ended"] = ended.RefusalFor(request with { Actor = Reporters.Identity }, _now);
        _refusals["ended_before_withdrawn"] = (ended with { Withdrawn = true, Granted = false }).RefusalFor(request, _now);
        _refusals["withdrawn_before_grant"] = (live with { Withdrawn = true, Granted = false }).RefusalFor(request, _now);
        _refusals["grant_before_identity"] = (live with { Granted = false, SharedIdentity = null }).RefusalFor(request, _now);
    }

    [Fact] void should_refuse_an_unreported_issue() => _refusals["missing"].ShouldEqual(SupportSessionRefusal.IssueNotReported);
    [Fact] void should_refuse_another_subject() => _refusals["subject"].ShouldEqual(SupportSessionRefusal.NotTheReporter);
    [Fact] void should_refuse_the_reporter_as_actor() => _refusals["self"].ShouldEqual(SupportSessionRefusal.ActorIsTheReporter);
    [Fact] void should_distinguish_identity_providers() => _refusals["provider"].ShouldBeNull();
    [Fact] void should_refuse_an_ended_session() => _refusals["ended"].ShouldEqual(SupportSessionRefusal.SessionEnded);
    [Fact] void should_refuse_a_withdrawn_issue() => _refusals["withdrawn"].ShouldEqual(SupportSessionRefusal.IssueWithdrawn);
    [Fact] void should_refuse_a_revoked_grant() => _refusals["revoked"].ShouldEqual(SupportSessionRefusal.NoLiveGrant);
    [Fact] void should_refuse_an_expired_grant() => _refusals["expired"].ShouldEqual(SupportSessionRefusal.NoLiveGrant);
    [Fact] void should_refuse_at_the_exact_grant_end() => _refusals["boundary"].ShouldEqual(SupportSessionRefusal.NoLiveGrant);
    [Fact] void should_require_shared_identity() => _refusals["unshared"].ShouldEqual(SupportSessionRefusal.IdentityNotShared);
    [Fact] void should_require_the_reporters_shared_identity() => _refusals["wrong_identity"].ShouldEqual(SupportSessionRefusal.IdentityNotShared);
    [Fact] void should_allow_a_live_grant() => _refusals["live"].ShouldBeNull();
    [Fact] void should_check_subject_before_actor() => _refusals["subject_before_self"].ShouldEqual(SupportSessionRefusal.NotTheReporter);
    [Fact] void should_check_actor_before_session_end() => _refusals["self_before_ended"].ShouldEqual(SupportSessionRefusal.ActorIsTheReporter);
    [Fact] void should_check_session_end_before_withdrawal() => _refusals["ended_before_withdrawn"].ShouldEqual(SupportSessionRefusal.SessionEnded);
    [Fact] void should_check_withdrawal_before_grant() => _refusals["withdrawn_before_grant"].ShouldEqual(SupportSessionRefusal.IssueWithdrawn);
    [Fact] void should_check_grant_before_identity() => _refusals["grant_before_identity"].ShouldEqual(SupportSessionRefusal.NoLiveGrant);
}
#endif
