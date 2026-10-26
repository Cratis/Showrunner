// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue.when_withdrawing_an_issue;

public class and_the_issue_is_already_withdrawn : an_issue_command<WithdrawIssue>
{
    void Establish() => GivenReported(new ReporterWithdrewIssue());

    async Task Because() => _result = await _scenario.Execute(new WithdrawIssue(_issueId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
