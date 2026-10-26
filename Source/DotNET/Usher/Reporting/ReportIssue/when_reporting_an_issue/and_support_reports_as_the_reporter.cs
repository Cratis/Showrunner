// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Reporting.ReportIssue.when_reporting_an_issue;

/// <summary>
/// A session is for looking into the one issue the reporter granted access to: support signed in as the reporter can
/// not open new issues in the reporter's name, whether or not it shares or grants anything.
/// </summary>
public class and_support_reports_as_the_reporter : an_issue_command<ReportIssue>
{
    protected override ClaimsPrincipal Principal => Reporters.SupportSignedInAs(Reporters.Subject);
    protected override VerifiedForward? Session => SessionOn(IssueId.New());

    async Task Because() => _result = await _scenario.Execute(Reports.Issue(_issueId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_say_support_can_not_report_in_the_reporters_name() => _result.ValidationResults.ShouldContain(_ => _.Message == "Support signed in as the reporter can not report in the reporter's name.");
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
