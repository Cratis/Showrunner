// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Conversation.AddDetails.when_adding_details;

/// <summary>
/// A subject is unique only within its identity provider: somebody with the reporter's subject at another provider
/// did not report the issue.
/// </summary>
public class and_someone_with_the_reporters_subject_at_another_provider_adds_them : an_issue_command<AddDetails>
{
    protected override ClaimsPrincipal Principal => Reporters.SignedInAs(Reporters.Subject, Reporters.AnotherIdentityProvider);

    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new AddDetails(_issueId, "It happens every time."));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
