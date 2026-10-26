// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.given;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.when_forwarding;

public class in_a_live_session : a_forwarded_request
{
    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Identity), new ReporterSharedIdentity(Reporters.Reporter), new ReporterGrantedAccess(Now.AddHours(1)));
        await Forwarded();
    }

    [Fact] void should_pass_the_request_on() => _passedOn.ShouldBeTrue();
    [Fact] void should_keep_the_verified_assertion() => Forward!.Assertion.Issue.ShouldEqual(_issueId.Value.ToString());
    [Fact] void should_decide_the_session_is_live() => Forward!.IsLive.ShouldBeTrue();
}
#endif
