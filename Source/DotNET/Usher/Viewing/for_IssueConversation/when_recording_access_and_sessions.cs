// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;

namespace Cratis.Showrunner.Usher.Viewing.for_IssueConversation;

public class when_recording_access_and_sessions : a_read_model<IssueConversation>
{
    IssueConversation _granted;
    IssueConversation _staleLapse;
    IssueConversation _started;
    IssueConversation _duplicateStart;
    IssueConversation _ended;
    IssueConversation _duplicateEnd;
    IssueConversation _withdrawn;
    IssueConversation _lapsed;
    IssueConversation _revoked;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Identity), new ReporterSharedIdentity(Reporters.Reporter), new ReporterGrantedAccess(Start.AddHours(1)));
        _granted = _scenario.Instance!;
        await Happened(new AccessGrantLapsed(Start.AddMinutes(30)));
        _staleLapse = _scenario.Instance!;
        await Happened(new SupportSignedInAsReporter(Reporters.SessionId, IssueEvents.Support, IssueEvents.SupportPerson));
        _started = _scenario.Instance!;
        await Happened(new SupportSignedInAsReporter(Reporters.SessionId, IssueEvents.Agent, IssueEvents.SupportPerson));
        _duplicateStart = _scenario.Instance!;
        await Happened(new SupportSessionEnded(Reporters.SessionId));
        _ended = _scenario.Instance!;
        await Happened(new SupportSessionEnded(Reporters.SessionId), new SupportSessionEnded("unknown-session"));
        _duplicateEnd = _scenario.Instance!;
        await Happened(new AccessGrantLapsed(Start.AddHours(1)));
        _lapsed = _scenario.Instance!;
        await Happened(new ReporterRevokedAccess());
        _revoked = _scenario.Instance!;
        await Happened(new ReporterGrantedAccess(Start.AddHours(2)), new ReporterWithdrewIssue());
        _withdrawn = _scenario.Instance!;
    }

    [Fact] void should_open_with_the_reporters_title() => _granted.Title.Value.ShouldEqual("The invoice is not sent");
    [Fact] void should_remember_shared_identity() => _granted.IdentityShared.ShouldBeTrue();
    [Fact] void should_record_the_grant() => _granted.Access.State.ShouldEqual(AccessState.Granted);
    [Fact] void should_ignore_an_earlier_grants_lapse() => _staleLapse.Entries.ShouldContainOnly(_granted.Entries.ToArray());
    [Fact] void should_start_one_session() => _started.Access.Sessions.Count().ShouldEqual(1);
    [Fact] void should_attribute_the_start() => _started.Entries.Last().Session.ShouldEqual(Reporters.SessionId);
    [Fact] void should_ignore_a_duplicate_start() => _duplicateStart.Access.Sessions.ShouldContainOnly(_started.Access.Sessions.ToArray());
    [Fact] void should_end_the_session() => _ended.Access.Sessions.Single().Ended.ShouldBeTrue();
    [Fact] void should_keep_the_original_author_on_end() => _ended.Entries.Last().AuthorName.ShouldEqual(IssueEvents.Support.Name);
    [Fact] void should_ignore_duplicate_and_unknown_ends() => _duplicateEnd.Entries.ShouldContainOnly(_ended.Entries.ToArray());
    [Fact] void should_lapse_the_current_grant() => _lapsed.Access.State.ShouldEqual(AccessState.Lapsed);
    [Fact] void should_record_revocation() => _revoked.Access.State.ShouldEqual(AccessState.Revoked);
    [Fact] void should_end_access_on_withdrawal() => _withdrawn.Access.State.ShouldEqual(AccessState.EndedByWithdrawal);
}
#endif
