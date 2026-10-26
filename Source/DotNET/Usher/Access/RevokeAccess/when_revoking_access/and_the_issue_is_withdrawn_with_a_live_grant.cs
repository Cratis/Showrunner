// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;

namespace Cratis.Showrunner.Usher.Access.RevokeAccess.when_revoking_access;

/// <summary>
/// Revoking only narrows what support can do, so the reporter can still take access back after withdrawing.
/// </summary>
public class and_the_issue_is_withdrawn_with_a_live_grant : an_issue_command<RevokeAccess>
{
    void Establish() => GivenReported(
        new ReporterSharedIdentity(Reporters.Reporter),
        new ReporterGrantedAccess(Now.AddHours(1)),
        new ReporterWithdrewIssue());

    async Task Because() => _result = await _scenario.Execute(new RevokeAccess(_issueId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
}
#endif
