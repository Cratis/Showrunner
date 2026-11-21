// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net.Sockets;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Probe.for_ProbeEvaluator;

public class when_an_endpoint_is_down : a_probe
{
    ProbeReport _report;

    void Establish()
    {
        _status = request => request.RequestUri!.Host == "app.cratis.direct" ? 200 : 401;
        _failure = request => request.RequestUri!.AbsolutePath.EndsWith("subscribe", StringComparison.Ordinal)
            ? new HttpRequestException("request to https://app.cratis.studio/secret-path failed", new SocketException((int)SocketError.ConnectionRefused))
            : null;
        _handler = new StubHttpHandler(request => _failure(request) is { } error ? throw error : new HttpResponseMessage(request.RequestUri!.AbsolutePath == "/health" && request.RequestUri.Host == "app.cratis.studio" ? System.Net.HttpStatusCode.OK : (System.Net.HttpStatusCode)_status(request)));
    }

    async Task Because() => _report = await Evaluator.Evaluate(Targets, TimeSpan.FromSeconds(30), CancellationToken.None);

    [Fact] void should_be_down() => _report.IsDown.ShouldBeTrue();
    [Fact] void should_name_every_endpoint_that_failed_both_rounds() => _report.Failing.Select(result => result.Description).ShouldContainOnly("studio/subscribe: ConnectionRefused", "studio/health: 200");
    [Fact] void should_keep_only_a_stable_error_code_and_never_the_message() => _report.FailingText.ShouldNotContain("secret-path");
    [Fact] void should_treat_a_different_status_as_a_failure() => _report.Failing.Single(result => result.Check == "health").Ok.ShouldBeFalse();
    [Fact] void should_list_the_failures_one_per_line() => _report.FailingText.Split('\n').Length.ShouldEqual(2);
}
#endif
