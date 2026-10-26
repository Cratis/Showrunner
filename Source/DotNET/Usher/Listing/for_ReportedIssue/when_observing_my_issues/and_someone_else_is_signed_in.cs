// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue.when_observing_my_issues;

public class and_someone_else_is_signed_in : a_query<ReportedIssue>
{
    protected override ClaimsPrincipal Principal => Reporters.SignedInAs(Reporters.SomeoneElse);

    void Establish() => Given(IssueId.New(), IssueEvents.Described(Reporters.Subject));

    async Task Because() => _result = await _scenario.Perform(nameof(ReportedIssue.MyIssues));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_not_return_the_reporters_issues() => ((IEnumerable<ReportedIssue>)_result.Data).ShouldBeEmpty();
}
#endif
