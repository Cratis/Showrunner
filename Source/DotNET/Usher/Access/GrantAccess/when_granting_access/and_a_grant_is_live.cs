// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.GrantAccess.when_granting_access;

public class and_a_grant_is_live : an_issue_command<GrantAccess>
{
    void Establish() => GivenReported(
        new ReporterSharedIdentity(Reporters.Reporter),
        new ReporterGrantedAccess(Now.AddMinutes(30)));

    async Task Because() => _result = await _scenario.Execute(new GrantAccess(_issueId, AccessDuration.OneDay));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
