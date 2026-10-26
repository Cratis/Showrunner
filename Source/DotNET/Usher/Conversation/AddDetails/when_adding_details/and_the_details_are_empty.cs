// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Conversation.AddDetails.when_adding_details;

public class and_the_details_are_empty : an_issue_command<AddDetails>
{
    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new AddDetails(_issueId, Details.NotSet));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
