// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Lifecycle.ReopenIssue.when_reopening_an_issue;

public class and_the_issue_is_not_resolved : an_issue_command<ReopenIssue>
{
    void Establish() => GivenReported(new SupportStartedWork());

    async Task Because() => _result = await _scenario.Execute(new ReopenIssue(_issueId, "It still does not send."));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
