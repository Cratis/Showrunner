// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Conversation.AddDetails.when_adding_details;

/// <summary>
/// AuthProxy still vouches for the session, but the reporter revoked the grant: support may no longer act, from the
/// moment the revocation is recorded rather than at AuthProxy's next grant check.
/// </summary>
public class and_support_adds_them_as_the_reporter_in_a_session_that_is_no_longer_live : an_issue_command<AddDetails>
{
    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);
    protected override VerifiedForward? Session => Reporters.SessionOn(_issueId, SupportSessionRefusal.NoLiveGrant);

    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new AddDetails(_issueId, "It happens every Monday."));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
