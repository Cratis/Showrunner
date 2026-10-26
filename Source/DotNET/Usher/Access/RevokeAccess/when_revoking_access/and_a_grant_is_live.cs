// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.RevokeAccess.when_revoking_access;

public class and_a_grant_is_live : an_issue_command<RevokeAccess>
{
    void Establish() => GivenReported(
        new ReporterSharedIdentity(Reporters.Reporter),
        new ReporterGrantedAccess(Now.AddHours(1)));

    async Task Because() => _result = await _scenario.Execute(new RevokeAccess(_issueId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_reporter_revoked_access() =>
        await _scenario.ShouldHaveAppendedEvent<RevokeAccess, ReporterRevokedAccess>(_issueId);
}
#endif
