// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Identity.ShareIdentity.when_sharing_identity;

public class and_someone_else_reported_the_issue : an_issue_command<ShareIdentity>
{
    void Establish() => GivenReportedBy(Reporters.SomeoneElsesIdentity);

    async Task Because() => _result = await _scenario.Execute(new ShareIdentity(_issueId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
