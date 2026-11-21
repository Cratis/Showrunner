// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json.Nodes;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_github_fails : an_actions_audit
{
    const string Token = "ghp_this-is-a-credential";

    IReadOnlyCollection<Alerts.Finding> _serverError;
    IReadOnlyCollection<Alerts.Finding> _rateLimited;
    IReadOnlyCollection<Alerts.Finding> _unexpected;
    IReadOnlyCollection<Alerts.Finding> _insideTheBackOff;
    double _serverRetry;
    double _rateLimitedRetry;
    double _shortResetRetry;
    int _requestsInsideTheBackOff;

    double RetryAt() => (double)JsonNode.Parse(_audit.Persist()!)!["retry_at"]!;

    async Task Because()
    {
        var start = _time.Now.ToUnixTimeSeconds();

        _github.Fail(Organization, new GitHubApiException(500));
        await Run();
        _serverError = _findings;
        _serverRetry = RetryAt() - start;

        // A rate limit waits at least a day, and a reset or retry-after header can only lengthen that.
        _audit = null;
        _github.Fail(Organization, new GitHubApiException(403, "0", "5", (start + 2 * 24 * 3600).ToString()));
        await Run();
        _rateLimited = _findings;
        _rateLimitedRetry = RetryAt() - start;

        _audit = null;
        _github.Fail(Organization, new GitHubApiException(429, "0", "5", (start + 60).ToString()));
        await Run();
        _shortResetRetry = RetryAt() - start;

        // An unexpected failure never reaches the finding text, which could otherwise carry a URL or a credential.
        _audit = null;
        _github.Fail(Organization, new InvalidOperationException($"request to https://api.github.com/orgs?access_token={Token} failed"));
        await Run();
        _unexpected = _findings;

        // And the audit does not retry in a loop: inside the back-off it does not ask again.
        _github.Requests.Clear();
        _time.Advance(TimeSpan.FromMinutes(10));
        await Run();
        _insideTheBackOff = _findings;
        _requestsInsideTheBackOff = _github.Requests.Count;
    }

    [Fact] void should_say_the_audit_is_unavailable_with_the_status() => _serverError.Single().Text.ShouldEqual("GitHub Actions audit unavailable: HTTP 500");
    [Fact] void should_retry_a_server_error_in_an_hour() => _serverRetry.ShouldEqual(3600);
    [Fact] void should_say_the_audit_is_unavailable_when_rate_limited() => _rateLimited.Single().Text.ShouldEqual("GitHub Actions audit unavailable: HTTP 403");
    [Fact] void should_wait_until_the_reset_when_that_is_later_than_a_day() => _rateLimitedRetry.ShouldEqual(2 * 24 * 3600);
    [Fact] void should_never_wait_less_than_a_day_when_rate_limited() => _shortResetRetry.ShouldEqual(24 * 3600);
    [Fact] void should_not_put_the_failure_in_the_finding() => _unexpected.Single().Text.ShouldEqual("GitHub Actions audit unavailable: request or response failed");
    [Fact] void should_never_disclose_the_credential() => _unexpected.Single().Text.ShouldNotContain(Token);
    [Fact] void should_keep_reporting_the_failure_inside_the_back_off() => _insideTheBackOff.Single().Key.ShouldEqual("actions-audit:unavailable");
    [Fact] void should_not_ask_github_again_inside_the_back_off() => _requestsInsideTheBackOff.ShouldEqual(0);
}
#endif
