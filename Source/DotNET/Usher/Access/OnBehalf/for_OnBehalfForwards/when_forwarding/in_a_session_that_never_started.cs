// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.given;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.when_forwarding;

public class in_a_session_that_never_started : a_forwarded_request
{
    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Identity),
            new ReporterSharedIdentity(Reporters.Reporter),
            new ReporterGrantedAccess(Now.AddHours(1)),
            new SupportSessionPermitted(Reporters.SessionId));
        await Forwarded();
    }

    [Fact] void should_pass_the_request_on() => _passedOn.ShouldBeTrue();
    [Fact] void should_decide_the_session_is_not_live() => Forward!.IsLive.ShouldBeFalse();
    [Fact] void should_say_why() => Forward!.Refusal.ShouldEqual(SupportSessionRefusal.SessionNotStarted);
}
#endif
