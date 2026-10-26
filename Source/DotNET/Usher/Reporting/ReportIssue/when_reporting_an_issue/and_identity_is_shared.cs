// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Reporting.ReportIssue.when_reporting_an_issue;

public class and_identity_is_shared : an_issue_command<ReportIssue>
{
    async Task Because() => _result = await _scenario.Execute(new ReportIssue(
        _issueId,
        IssueEvents.Location,
        [],
        "The invoice is not sent",
        "I pressed send and nothing happened.",
        "1.2.3",
        true,
        false,
        AccessDuration.OneHour));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_the_report_then_the_identity() =>
        _scenario.AppendedEvents.Select(_ => _.Event.Content.GetType()).ShouldEqual([typeof(ReporterDescribedIssue), typeof(ReporterSharedIdentity)]);

    [Fact]
    async Task should_share_who_the_signed_in_reporter_is() =>
        await _scenario.ShouldHaveAppendedEvent<ReportIssue, ReporterSharedIdentity>(_issueId, e => e.Reporter == Reporters.Reporter);
}
#endif
