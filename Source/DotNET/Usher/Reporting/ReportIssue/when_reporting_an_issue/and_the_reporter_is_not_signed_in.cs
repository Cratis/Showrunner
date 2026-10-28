// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Reporting.ReportIssue.when_reporting_an_issue;

public class and_the_reporter_is_not_signed_in : an_issue_command<ReportIssue>
{
    protected override ClaimsPrincipal Principal => Reporters.Anonymous();

    async Task Because() => _result = await _scenario.Execute(Reports.Issue(_issueId));

    [Fact] void should_not_be_authorized() => _result.ShouldNotBeAuthorized();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
