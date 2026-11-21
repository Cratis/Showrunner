// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_a_remedy_is_not_authorized : a_direct_link
{
    async Task Because()
    {
        PublishReadiness(RemedyPolicyMode.Review);
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
        await Pass();
    }

    [Fact] void should_ask_direct_once_not_on_every_pass() => _direct.RemedyRequests.Count.ShouldEqual(1);
    [Fact] void should_not_run_anything_until_a_person_reviewed_it() => _deployments.Restarted.ShouldBeEmpty();
}
#endif
