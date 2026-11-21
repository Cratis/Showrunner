// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_a_remedy_is_authorized : a_direct_link
{
    async Task Because()
    {
        PublishReadiness();
        _direct.AutoAuthorize = true;
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
        _endpointStatus.Clear();
        _time.Advance(TimeSpan.FromMinutes(1));
        await Pass();
    }

    [Fact] void should_ask_direct_once_citing_the_failing_observation() => _direct.RemedyRequests.Single().Evidence.ShouldEqual(_direct.Events.First(envelope => envelope.Type == WatchdogEventTypes.CheckFailed).EventId);
    [Fact] void should_run_the_bound_action_once() => _deployments.Restarted.ShouldEqual(["shop/web"]);
    [Fact] void should_report_the_attempt_under_the_direct_authorization() => Sent<RemedyAttempted>().Single().Authorization.ShouldEqual(_direct.RemedyRequests.Single().Request);
    [Fact] void should_report_that_it_was_not_offline() => Sent<RemedyAttempted>().Single().Offline.ShouldBeFalse();
    [Fact] void should_report_success() => Sent<RemedySucceeded>().Single().Remedy.ShouldEqual(Restart);
    [Fact] void should_report_verification_when_the_check_passes_again() => Sent<RemedyVerified>().Single().Check.ShouldEqual(Readiness);
    [Fact] void should_report_the_recovery() => Sent<CheckRecovered>().Single().Check.ShouldEqual(Readiness);
}
#endif
