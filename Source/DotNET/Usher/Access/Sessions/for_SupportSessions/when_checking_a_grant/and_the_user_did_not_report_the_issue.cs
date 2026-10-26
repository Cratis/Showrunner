// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

public class and_the_user_did_not_report_the_issue : a_refusal
{
    async Task Because()
    {
        await Granted();
        _result = await _sessions.Check(_eventStore, Request with { Subject = Reporters.SomeoneElse });
    }

    [Fact] void should_not_be_live() => _result.IsSuccess.ShouldBeFalse();
    [Fact] void should_say_why() => Refusal().ShouldEqual(SupportSessionRefusal.NotTheReporter);
}
#endif
