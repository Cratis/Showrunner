// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Conversation.AddDetails.when_adding_details;

public class and_support_adds_them_as_the_reporter_without_a_verified_session : an_issue_command<AddDetails>
{
    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);

    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new AddDetails(_issueId, "It happens every Monday."));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
