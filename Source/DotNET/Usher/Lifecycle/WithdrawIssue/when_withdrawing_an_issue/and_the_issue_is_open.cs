// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue.when_withdrawing_an_issue;

public class and_the_issue_is_open : an_issue_command<WithdrawIssue>
{
    void Establish() => GivenReported(new SupportAcceptedIssue());

    async Task Because() => _result = await _scenario.Execute(new WithdrawIssue(_issueId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_reporter_withdrew_it() =>
        await _scenario.ShouldHaveAppendedEvent<WithdrawIssue, ReporterWithdrewIssue>(_issueId);
}
#endif
