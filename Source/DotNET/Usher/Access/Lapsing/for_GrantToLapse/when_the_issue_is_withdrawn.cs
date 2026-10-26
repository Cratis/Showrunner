// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantToLapse;

public class when_the_issue_is_withdrawn : a_read_model<GrantToLapse>
{
    async Task Because() => await Happened(IssueEvents.Described(Reporters.Identity), new ReporterGrantedAccess(Start.AddHours(1)), new ReporterWithdrewIssue());

    [Fact] void should_have_no_grant_to_lapse() => _scenario.Instance.ShouldBeNull();
}
#endif
