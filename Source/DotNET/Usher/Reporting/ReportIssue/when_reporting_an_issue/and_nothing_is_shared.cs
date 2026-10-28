// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Reporting.ReportIssue.when_reporting_an_issue;

public class and_nothing_is_shared : an_issue_command<ReportIssue>
{
    async Task Because() => _result = await _scenario.Execute(new ReportIssue(
        _issueId,
        IssueEvents.Location,
        [],
        "The invoice is not sent",
        "I pressed send and nothing happened.",
        "1.2.3",
        false,
        false,
        AccessDuration.OneHour));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_reporter_described_the_issue() =>
        await _scenario.ShouldHaveAppendedEvent<ReportIssue, ReporterDescribedIssue>(_issueId, e =>
            e.Summary == "The invoice is not sent" &&
            e.Description == "I pressed send and nothing happened." &&
            e.Location == IssueEvents.Location &&
            e.ApplicationName == "Ledger" &&
            e.ApplicationVersion == "1.2.3" &&
            e.ReportedBy == Reporters.Identity);

    [Fact] void should_not_share_identity() => _scenario.AppendedEvents.ShouldNotContain(_ => _.Event.Content is ReporterSharedIdentity);
}
#endif
