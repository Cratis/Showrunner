// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.GrantAccess.when_granting_access;

/// <summary>
/// A grant is no longer live once its end has passed, even before the lapse is recorded.
/// </summary>
public class and_the_last_grant_has_passed_its_end : an_issue_command<GrantAccess>
{
    void Establish() => GivenReported(
        new ReporterSharedIdentity(Reporters.Reporter),
        new ReporterGrantedAccess(Now.AddMinutes(-1)));

    async Task Because() => _result = await _scenario.Execute(new GrantAccess(_issueId, AccessDuration.OneHour));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_grant_access_again() =>
        await _scenario.ShouldHaveAppendedEvent<GrantAccess, ReporterGrantedAccess>(_issueId, e => e.ValidUntil.Value == Now.AddHours(1));
}
#endif
