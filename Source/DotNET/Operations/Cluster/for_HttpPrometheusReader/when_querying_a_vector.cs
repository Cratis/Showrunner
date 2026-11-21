// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Cluster.for_HttpPrometheusReader;

public class when_querying_a_vector : Specification
{
    StubHttpHandler _handler;
    IReadOnlyList<PrometheusSample> _samples;

    void Establish() => _handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("{\"status\":\"success\",\"data\":{\"resultType\":\"vector\",\"result\":[{\"metric\":{\"node\":\"a\"},\"value\":[1700000000.5,\"0.86\"]}]}}", Encoding.UTF8, "application/json")
    });

    async Task Because() => _samples = await new HttpPrometheusReader(new HttpClient(_handler), new Uri("http://prometheus:9090")).Query("up{a=\"b\"}", TimeSpan.FromSeconds(2), CancellationToken.None);

    [Fact] void should_read_the_labels() => _samples.Single().Label("node").ShouldEqual("a");
    [Fact] void should_read_the_value() => _samples.Single().Value.ShouldEqual(0.86);
    [Fact] void should_call_the_instant_query_endpoint() => _handler.Requests.Single().Uri.AbsolutePath.ShouldEqual("/api/v1/query");
    [Fact] void should_escape_the_expression() => _handler.Requests.Single().Uri.Query.ShouldEqual("?query=up%7Ba%3D%22b%22%7D");
}
#endif
