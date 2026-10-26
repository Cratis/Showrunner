// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Lifecycle.WithdrawIssue;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

/// <summary>
/// The grant was given and had not passed its end when the reporter withdrew the issue: withdrawing ends it.
/// </summary>
public class and_the_issue_was_withdrawn : a_refusal
{
    async Task Because()
    {
        await Granted();
        await Happened(new ReporterWithdrewIssue());
        _result = await _sessions.Check(_eventStore, Request);
    }

    [Fact] void should_not_be_live() => _result.IsSuccess.ShouldBeFalse();
    [Fact] void should_say_why() => Refusal().ShouldEqual(SupportSessionRefusal.IssueWithdrawn);
}
#endif
