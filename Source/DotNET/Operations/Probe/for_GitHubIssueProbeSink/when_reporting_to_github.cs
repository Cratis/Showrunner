// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Probe.for_GitHubIssueProbeSink;

public class when_reporting_to_github : Specification
{
    static readonly ProbeReport _ok = new([[]], [], []);
    static readonly ProbeReport _down = new([[]], [new ProbeResult("direct", "health", false, 503, null, 1)], []);

    StubHttpHandler _opened;
    StubHttpHandler _updated;
    StubHttpHandler _closed;
    StubHttpHandler _quiet;
    Exception _failure;

    static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    static StubHttpHandler Github(string openIssues) => new(request => request.Method == HttpMethod.Get ? Json(openIssues) : Json("{}", HttpStatusCode.Created));

    async Task Because()
    {
        _opened = Github("[]");
        await new GitHubIssueProbeSink(new HttpClient(_opened), "Cratis/Infrastructure", "ghp_secret").Report(_down, CancellationToken.None);

        _updated = Github("[{\"number\":12}]");
        await new GitHubIssueProbeSink(new HttpClient(_updated), "Cratis/Infrastructure", "ghp_secret").Report(_down, CancellationToken.None);

        _closed = Github("[{\"number\":12}]");
        await new GitHubIssueProbeSink(new HttpClient(_closed), "Cratis/Infrastructure", "ghp_secret").Report(_ok, CancellationToken.None);

        _quiet = Github("[]");
        await new GitHubIssueProbeSink(new HttpClient(_quiet), "Cratis/Infrastructure", "ghp_secret").Report(_ok, CancellationToken.None);

        _failure = await Record.ExceptionAsync(() => new GitHubIssueProbeSink(new HttpClient(StubHttpHandler.Answering(HttpStatusCode.Forbidden)), "Cratis/Infrastructure", "ghp_secret").Report(_down, CancellationToken.None));
    }

    [Fact] void should_make_sure_the_label_exists_first() => _opened.Requests[0].Uri.AbsolutePath.ShouldEqual("/repos/Cratis/Infrastructure/labels");
    [Fact] void should_open_an_issue_when_something_is_down_and_none_is_open() => _opened.Requests.Last().Uri.AbsolutePath.ShouldEqual("/repos/Cratis/Infrastructure/issues");
    [Fact] void should_title_it_with_the_failing_endpoints() => _opened.Requests.Last().Body.ShouldContain("Public endpoint probe failing: direct/health");
    [Fact] void should_label_it() => _opened.Requests.Last().Body.ShouldContain("\"labels\":[\"probe-down\"]");
    [Fact] void should_comment_on_the_open_issue_while_it_stays_down() => _updated.Requests.Last().Uri.AbsolutePath.ShouldEqual("/repos/Cratis/Infrastructure/issues/12/comments");
    [Fact] void should_not_open_a_second_issue() => _updated.Requests.Count(request => request.Uri.AbsolutePath.EndsWith("/issues", StringComparison.Ordinal) && request.Method == HttpMethod.Post).ShouldEqual(0);
    [Fact] void should_close_the_issue_when_everything_answers() => (_closed.Requests.Last().Method, _closed.Requests.Last().Uri.AbsolutePath, _closed.Requests.Last().Body).ShouldEqual((HttpMethod.Patch, "/repos/Cratis/Infrastructure/issues/12", "{\"state\":\"closed\"}"));
    [Fact] void should_say_it_recovered_before_closing() => _closed.Requests.Any(request => request.Body.Contains("recovered")).ShouldBeTrue();
    [Fact] void should_do_nothing_when_all_is_well_and_nothing_is_open() => _quiet.Requests.Count.ShouldEqual(2);
    [Fact] void should_send_the_token_as_a_bearer() => _opened.Requests[0].Authorization.ShouldEqual("Bearer ghp_secret");
    [Fact] void should_fail_without_a_body_or_the_token() => _failure.Message.ShouldEqual("GitHub issue delivery failed");
}
#endif
