// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.RevokeAccess.when_revoking_access;

public class and_no_grant_was_given : an_issue_command<RevokeAccess>
{
    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new RevokeAccess(_issueId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
