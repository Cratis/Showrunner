// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_remedy_budget_is_spent : a_direct_link
{
    async Task Because()
    {
        PublishReadiness(attempts: 1);
        _direct.AutoAuthorize = true;
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
        _time.Advance(TimeSpan.FromMinutes(11));
        await Pass();
        _time.Advance(TimeSpan.FromMinutes(11));
        await Pass();
    }

    [Fact] void should_run_the_remedy_only_as_often_as_the_budget_allows() => _deployments.Restarted.Count.ShouldEqual(1);
    [Fact] void should_deny_the_authorization_it_cannot_honor_and_say_why() => Sent<RemedyDenied>().ShouldContain(denied => denied.Reason == "AuthorizationRecheck" && denied.Explanation.Contains("budget"));
    [Fact] void should_escalate_once_for_a_person_to_act() => Sent<RemedyEscalated>().Count().ShouldEqual(1);
}
#endif
