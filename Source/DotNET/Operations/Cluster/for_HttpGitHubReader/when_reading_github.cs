// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Cluster.for_HttpGitHubReader;

public class when_reading_github : Specification
{
    StubHttpHandler _handler;
    GitHubPage _page;
    GitHubApiException _limited;
    GitHubApiException _redirected;
    Exception _tooLarge;

    async Task Because()
    {
        var call = 0;
        _handler = new StubHttpHandler(_ =>
        {
            call++;
            switch (call)
            {
                case 1:
                    var ok = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"full_name\":\"Cratis/app\"}]", Encoding.UTF8, "application/json") };
                    ok.Headers.Add("Link", "<https://api.github.com/orgs/Cratis/repos?page=2>; rel=\"next\", <https://api.github.com/orgs/Cratis/repos?page=9>; rel=\"last\"");
                    return ok;
                case 2:
                    var limited = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    limited.Headers.Add("X-RateLimit-Remaining", "0");
                    limited.Headers.Add("X-RateLimit-Reset", "1900000000");
                    return limited;
                case 3:
                    var redirect = new HttpResponseMessage(HttpStatusCode.Found);
                    redirect.Headers.Location = new Uri("https://evil.example.com/");
                    return redirect;
                default:
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new string('x', HttpGitHubReader.MaximumBodyBytes + 1)) };
            }
        });
        var reader = new HttpGitHubReader(new HttpClient(_handler), "ghp_a-credential");
        var uri = new Uri("https://api.github.com/orgs/Cratis/repos?per_page=100");
        _page = await reader.Get(uri, TimeSpan.FromSeconds(10), CancellationToken.None);
        _limited = await Assert.ThrowsAsync<GitHubApiException>(() => reader.Get(uri, TimeSpan.FromSeconds(10), CancellationToken.None));
        _redirected = await Assert.ThrowsAsync<GitHubApiException>(() => reader.Get(uri, TimeSpan.FromSeconds(10), CancellationToken.None));
        _tooLarge = await Record.ExceptionAsync(() => reader.Get(uri, TimeSpan.FromSeconds(10), CancellationToken.None));
    }

    [Fact] void should_read_the_body() => _page.Body[0].GetProperty("full_name").GetString().ShouldEqual("Cratis/app");
    [Fact] void should_follow_the_next_link_not_the_last() => _page.Next.ShouldEqual("https://api.github.com/orgs/Cratis/repos?page=2");
    [Fact] void should_send_the_token_as_a_bearer() => _handler.Requests[0].Authorization.ShouldEqual("Bearer ghp_a-credential");
    [Fact] void should_identify_itself() => _handler.Requests[0].UserAgent.ShouldContain("Cratis-cluster-health-watchdog");
    [Fact] void should_report_a_rate_limit_with_its_headers() => (_limited.StatusCode, _limited.IsRateLimited, _limited.RateLimitReset).ShouldEqual((403, true, "1900000000"));
    [Fact] void should_treat_a_redirect_as_an_error_rather_than_follow_it() => _redirected.StatusCode.ShouldEqual(302);
    [Fact] void should_refuse_an_answer_that_is_too_large() => _tooLarge.ShouldBeOfExactType<GitHubResponseTooLargeException>();
    [Fact] void should_never_put_the_token_in_an_error() => _limited.Message.ShouldNotContain("ghp_a-credential");
}
#endif
