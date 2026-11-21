// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_bound_action_fails : a_direct_link
{
    async Task Because()
    {
        PublishReadiness();
        _direct.AutoAuthorize = true;
        _deployments.Fail = true;
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
    }

    [Fact] void should_report_the_attempt() => Sent<RemedyAttempted>().Count().ShouldEqual(1);
    [Fact] void should_report_the_failure_without_leaking_detail() => Sent<RemedyFailed>().Single().Reason.ShouldEqual("could not restart deployment shop/web: InvalidOperationException");
    [Fact] void should_not_report_success() => Sent<RemedySucceeded>().ShouldBeEmpty();
}
#endif
