// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Conversation.AddDetails.when_adding_details;

public class and_the_reporter_reported_the_issue : an_issue_command<AddDetails>
{
    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new AddDetails(_issueId, "It happens every Monday."));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_details() =>
        await _scenario.ShouldHaveAppendedEvent<AddDetails, ReporterAddedDetails>(_issueId, e => e.Details == "It happens every Monday.");
}
#endif
