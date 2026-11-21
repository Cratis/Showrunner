// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_http_remedies_are_enabled : a_direct_link
{
    void Establish() => _options = _options with { Remedies = [], AllowHttpRemedies = true };

    async Task Because()
    {
        _direct.Publish(
            [ReadinessCheck(Resource)],
            [RestartRemedy(Resource) with { Parameters = [new RemedyParameterDefinition("scope", "web")] }],
            new OfflinePolicy(true, false),
            new Dictionary<string, string> { ["probe"] = "probe-secret", ["admin"] = "admin-secret" });
        _direct.AutoAuthorize = true;
        _endpointStatus["/ready"] = HttpStatusCode.ServiceUnavailable;
        await Pass();
    }

    [Fact] void should_execute_the_pinned_request_with_its_own_credential() => _endpoints.Requests.ShouldContain(request => request.Method == HttpMethod.Post && request.Uri.ToString() == "https://shop.internal/restart" && request.Authorization == "Bearer admin-secret");
    [Fact] void should_send_the_fixed_body() => _endpoints.Requests.Single(request => request.Method == HttpMethod.Post).Body.ShouldEqual("{\"scope\":\"web\"}");
    [Fact] void should_report_the_status_it_got() => Sent<RemedySucceeded>().Single().StatusCode.ShouldEqual(200);
}
#endif
