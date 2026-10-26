// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Reporting.ReportIssue.when_reporting_an_issue;

public class and_the_summary_is_exactly_120_characters : an_issue_command<ReportIssue>
{
    async Task Because() => _result = await _scenario.Execute(Reports.Issue(_issueId) with { Summary = new string('x', 120) });

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
}
#endif
