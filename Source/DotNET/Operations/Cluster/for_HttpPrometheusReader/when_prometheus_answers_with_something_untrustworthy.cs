// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;

namespace Cratis.Showrunner.Operations.Cluster.for_HttpPrometheusReader;

public class when_prometheus_answers_with_something_untrustworthy : Specification
{
    readonly Dictionary<string, Exception?> _outcomes = [];

    static HttpPrometheusReader ReaderAnswering(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(new HttpClient(new given.StubHttpHandler(_ => new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") })), new Uri("http://prometheus:9090"));

    async Task Attempt(string name, string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        try
        {
            await ReaderAnswering(body, status).Query("up", TimeSpan.FromSeconds(2), CancellationToken.None);
            _outcomes[name] = null;
        }
        catch (Exception error)
        {
            _outcomes[name] = error;
        }
    }

    async Task Because()
    {
        await Attempt("error", "{\"status\":\"error\",\"error\":\"bad expression\"}");
        await Attempt("warnings", "{\"status\":\"success\",\"warnings\":[\"partial\"],\"data\":{\"resultType\":\"vector\",\"result\":[]}}");
        await Attempt("matrix", "{\"status\":\"success\",\"data\":{\"resultType\":\"matrix\",\"result\":[]}}");
        await Attempt("sample", "{\"status\":\"success\",\"data\":{\"resultType\":\"vector\",\"result\":[{\"metric\":{},\"value\":[1,\"NaN\"]}]}}");
        await Attempt("label", "{\"status\":\"success\",\"data\":{\"resultType\":\"vector\",\"result\":[{\"metric\":{\"a\":1},\"value\":[1,\"1\"]}]}}");
        await Attempt("http", "{}", HttpStatusCode.ServiceUnavailable);
    }

    [Fact] void should_refuse_an_error_status() => _outcomes["error"]!.Message.ShouldEqual("bad expression");
    [Fact] void should_refuse_warnings() => _outcomes["warnings"]!.Message.ShouldEqual("partial");
    [Fact] void should_refuse_a_result_that_is_not_a_vector() => _outcomes["matrix"]!.Message.ShouldContain("instant vector");
    [Fact] void should_refuse_a_value_that_is_not_finite() => _outcomes["sample"]!.Message.ShouldContain("unreadable sample");
    [Fact] void should_refuse_a_label_that_is_not_text() => _outcomes["label"]!.Message.ShouldContain("unreadable sample");
    [Fact] void should_refuse_an_http_failure() => _outcomes["http"].ShouldBeOfExactType<HttpRequestException>();
}
#endif
