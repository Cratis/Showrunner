// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_an_endpoint_answers_outside_the_expected_range : a_direct_link
{
    IReadOnlyCollection<Finding> _findings;

    async Task Because()
    {
        PublishReadiness();
        _endpointStatus["/ready"] = HttpStatusCode.MovedPermanently;
        var link = CreateLink();
        await link.Begin(CancellationToken.None);
        _findings = await new DirectEndpointCheck(link, new HttpClient(_endpoints)).Collect(new CheckContext(_time.Now, _time.Now, _time.Now.AddMinutes(1), _time), CancellationToken.None);
    }

    [Fact] void should_raise_a_finding_the_alert_channels_see_too() => _findings.Single().Key.ShouldEqual("direct-check:readiness");
    [Fact] void should_say_what_was_expected_without_a_secret() => _findings.Single().Text.ShouldEqual("Direct check 'readiness' failed: status 301 is outside 200-299");
}
#endif
