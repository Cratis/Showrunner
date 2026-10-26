// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Lifecycle.ReopenIssue.when_reopening_an_issue;

public class and_the_issue_is_resolved : an_issue_command<ReopenIssue>
{
    void Establish() => GivenReported(new SupportResolvedIssue("Fixed the send button."));

    async Task Because() => _result = await _scenario.Execute(new ReopenIssue(_issueId, "It still does not send."));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_reporter_reopened_it() =>
        await _scenario.ShouldHaveAppendedEvent<ReopenIssue, ReporterReopenedIssue>(_issueId, e => e.Reason == "It still does not send.");
}
#endif
