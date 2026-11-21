// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PublicUrlsCheck;

public class when_urls_answer_differently : a_check
{
    StubHttpHandler _handler;

    protected override IWatchdogCheck Check => new PublicUrlsCheck(
        new HttpClient(_handler),
        [new Uri("https://up.example.com/"), new Uri("https://protected.example.com/"), new Uri("https://down.example.com/"), new Uri("https://flaky.example.com/"), new Uri("https://unreachable.example.com/")],
        _time);

    void Establish()
    {
        var flakyAttempts = 0;
        _handler = new StubHttpHandler(request => request.RequestUri!.Host switch
        {
            "protected.example.com" => new HttpResponseMessage(HttpStatusCode.Unauthorized),
            "down.example.com" => new HttpResponseMessage(HttpStatusCode.BadGateway),
            "flaky.example.com" => ++flakyAttempts == 2 ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) : new HttpResponseMessage(HttpStatusCode.OK),
            "unreachable.example.com" => throw new HttpRequestException("connection to https://unreachable.example.com/ failed"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
        });
    }

    async Task Because() => await Run();

    [Fact] void should_not_report_a_url_that_answers() => Keys.ShouldNotContain("url:https://up.example.com/");
    [Fact] void should_treat_a_401_as_the_product_answering() => Keys.ShouldNotContain("url:https://protected.example.com/");
    [Fact] void should_report_a_url_that_fails_every_attempt() => _findings.First(finding => finding.Key == "url:https://down.example.com/").Text.ShouldContain("failed 3/3 attempts: HTTP 502");
    [Fact] void should_report_one_bad_attempt_out_of_three_because_that_is_a_load_balancer_with_a_dead_backend() => _findings.First(finding => finding.Key == "url:https://flaky.example.com/").Text.ShouldContain("failed 1/3 attempts");
    [Fact] void should_report_a_url_that_cannot_be_reached() => Keys.ShouldContain("url:https://unreachable.example.com/");
    [Fact] void should_redact_urls_that_a_transport_error_carries() => _findings.First(finding => finding.Key == "url:https://unreachable.example.com/").Text.ShouldContain("[redacted URL]");
    [Fact] void should_make_all_three_attempts_even_when_the_first_succeeds() => _handler.Requests.Count(request => request.Uri.Host == "up.example.com").ShouldEqual(3);
    [Fact] void should_identify_itself_with_the_watchdog_user_agent() => _handler.Requests.TrueForAll(request => request.UserAgent == "cratis-cluster-health-watchdog/1.0").ShouldBeTrue();
}
#endif
