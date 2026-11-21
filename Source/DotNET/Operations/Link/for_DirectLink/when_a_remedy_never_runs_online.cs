// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_a_remedy_never_runs_online : a_direct_link
{
    async Task Because()
    {
        PublishReadiness(RemedyPolicyMode.Never);
        _direct.AutoAuthorize = true;
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
    }

    [Fact] void should_only_observe_and_report() => (_direct.RemedyRequests.Count, _deployments.Restarted.Count).ShouldEqual((0, 0));
    [Fact] void should_still_report_the_failing_check() => Sent<CheckFailed>().Single().Check.ShouldEqual(Readiness);
}
#endif
