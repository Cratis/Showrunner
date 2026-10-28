// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.GrantAccess.when_granting_access;

public class and_the_last_grant_was_revoked : an_issue_command<GrantAccess>
{
    void Establish() => GivenReported(
        new ReporterSharedIdentity(Reporters.Reporter),
        new ReporterGrantedAccess(Now.AddHours(5)),
        new ReporterRevokedAccess());

    async Task Because() => _result = await _scenario.Execute(new GrantAccess(_issueId, AccessDuration.OneHour));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
}
#endif
