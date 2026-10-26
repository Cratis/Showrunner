// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Issues.for_Issue;

public class when_access_is_granted_and_revoked : a_read_model<Issue>
{
    static readonly ValidUntil _validUntil = new(Start.AddHours(2));
    bool _grantedAfterGrant;
    bool _grantedAfterRevoke;
    Issue _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Subject), new ReporterGrantedAccess(_validUntil));
        _grantedAfterGrant = _scenario.Instance!.AccessGranted;
        await Happened(new ReporterRevokedAccess());
        _issue = _scenario.Instance!;
        _grantedAfterRevoke = _issue.AccessGranted;
    }

    [Fact] void should_be_granted_after_the_grant() => _grantedAfterGrant.ShouldBeTrue();
    [Fact] void should_not_be_granted_after_the_revoke() => _grantedAfterRevoke.ShouldBeFalse();
    [Fact] void should_keep_the_end_of_the_last_grant() => _issue.AccessValidUntil.ShouldEqual(_validUntil);
    [Fact] void should_not_have_a_live_grant() => _issue.HasLiveGrant(Start).ShouldBeFalse();
}
#endif
