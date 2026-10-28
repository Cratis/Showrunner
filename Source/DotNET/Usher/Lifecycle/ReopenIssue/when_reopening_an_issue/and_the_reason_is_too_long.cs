// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;
using Cratis.Showrunner.Usher.Reporting;

namespace Cratis.Showrunner.Usher.Lifecycle.ReopenIssue.when_reopening_an_issue;

public class and_the_reason_is_too_long : an_issue_command<ReopenIssue>
{
    void Establish() => GivenReported(new SupportResolvedIssue("Fixed the send button."));

    async Task Because() => _result = await _scenario.Execute(new ReopenIssue(_issueId, new string('x', ReportLimits.ReopenReasonLength + 1)));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
