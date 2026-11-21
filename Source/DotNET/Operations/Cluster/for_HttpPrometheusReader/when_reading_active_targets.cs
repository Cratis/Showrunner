// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Cluster.for_HttpPrometheusReader;

public class when_reading_active_targets : Specification
{
    StubHttpHandler _handler;
    IReadOnlyList<ScrapeTarget> _targets;

    void Establish() => _handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("{\"status\":\"success\",\"data\":{\"activeTargets\":[{\"labels\":{\"job\":\"j\",\"instance\":\"i\"},\"health\":\"down\",\"lastError\":\"sample limit exceeded\"}]}}", Encoding.UTF8, "application/json")
    });

    async Task Because() => _targets = await new HttpPrometheusReader(new HttpClient(_handler), new Uri("http://prometheus:9090")).ActiveTargets(TimeSpan.FromSeconds(2), CancellationToken.None);

    [Fact] void should_read_the_target() => _targets.Single().ShouldEqual(new ScrapeTarget("j", "i", "down", "sample limit exceeded"));
    [Fact] void should_ask_for_active_targets_only() => _handler.Requests.Single().Uri.Query.ShouldEqual("?state=active");
}
#endif
