// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_a_remedy_has_no_local_binding : a_direct_link
{
    void Establish() => _options = _options with { Remedies = [] };

    async Task Because()
    {
        PublishReadiness();
        _direct.AutoAuthorize = true;
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
    }

    [Fact] void should_not_guess_at_an_action() => (_deployments.Restarted.Count, _endpoints.Requests.Count(request => request.Method == HttpMethod.Post)).ShouldEqual((0, 0));
    [Fact] void should_report_that_it_was_denied_for_lack_of_an_executor() => Sent<RemedyDenied>().Single().Reason.ShouldEqual("NoExecutor");
    [Fact] void should_not_count_a_denial_as_an_attempt() => Sent<RemedyAttempted>().ShouldBeEmpty();
}
#endif
