// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.GrantAccess.when_granting_access;

public class and_support_grants_it_as_the_reporter : an_issue_command<GrantAccess>
{
    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);
    protected override OnBehalfAssertion? Session => SessionOn(_issueId);

    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new GrantAccess(_issueId, AccessDuration.SevenDays));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_say_only_the_reporter_can_grant_access() => _result.ValidationResults.ShouldContain(_ => _.Message == "Only the reporter can grant support access.");
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
