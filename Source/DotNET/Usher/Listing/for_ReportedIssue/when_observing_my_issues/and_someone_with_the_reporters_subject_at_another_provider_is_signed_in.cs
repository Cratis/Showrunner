// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue.when_observing_my_issues;

/// <summary>
/// A subject is unique only within its identity provider: somebody with the reporter's subject at another provider is
/// somebody else and sees none of the reporter's issues.
/// </summary>
public class and_someone_with_the_reporters_subject_at_another_provider_is_signed_in : a_query<ReportedIssue>
{
    protected override ClaimsPrincipal Principal => Reporters.SignedInAs(Reporters.Subject, Reporters.AnotherIdentityProvider);

    void Establish() => Given(IssueId.New(), IssueEvents.Described(Reporters.Identity));

    async Task Because() => _result = await _scenario.Perform(nameof(ReportedIssue.MyIssues));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_not_return_the_reporters_issues() => ((IEnumerable<ReportedIssue>)_result.Data).ShouldBeEmpty();
}
#endif
