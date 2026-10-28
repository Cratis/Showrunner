// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Reporting.ReportError.when_reporting_an_error;

public class and_the_reporter_wrote_what_they_were_doing : an_issue_command<ReportError>
{
    async Task Because() => _result = await _scenario.Execute(Reports.Error(_issueId) with { Details = "I was sending the March invoice." });

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_the_error_then_the_details() =>
        _scenario.AppendedEvents.Select(_ => _.Event.Content.GetType()).ShouldEqual([typeof(ReporterFlaggedError), typeof(ReporterAddedDetails)]);

    [Fact]
    async Task should_add_what_they_wrote_as_details() =>
        await _scenario.ShouldHaveAppendedEvent<ReportError, ReporterAddedDetails>(_issueId, e => e.Details == "I was sending the March invoice.");
}
#endif
