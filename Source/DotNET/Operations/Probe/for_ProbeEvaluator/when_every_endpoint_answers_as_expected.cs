// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Probe.for_ProbeEvaluator;

public class when_every_endpoint_answers_as_expected : a_probe
{
    ProbeReport _report;

    void Establish() => _status = request => request.RequestUri!.Host == "app.cratis.direct" ? 200 : 401;

    async Task Because() => _report = await Evaluator.Evaluate(Targets, TimeSpan.FromSeconds(30), CancellationToken.None);

    [Fact] void should_not_be_down() => _report.IsDown.ShouldBeFalse();
    [Fact] void should_make_every_request_once() => _handler.Requests.Count.ShouldEqual(3);
    [Fact] void should_make_one_round() => _report.Rounds.Count.ShouldEqual(1);
    [Fact] void should_not_wait() => _time.Now.ShouldEqual(new DateTimeOffset(2026, 7, 1, 12, 0, 0, TimeSpan.Zero));
    [Fact] void should_join_the_base_address_and_the_path_without_a_double_slash() => _handler.Requests.Select(request => request.Uri.AbsoluteUri).ShouldContain("https://app.cratis.studio/health");
    [Fact] void should_send_the_body_of_the_check() => _handler.Requests.Single(request => request.Uri.AbsolutePath.EndsWith("subscribe", StringComparison.Ordinal)).Body.ShouldEqual("{}");
    [Fact] void should_identify_itself() => _handler.Requests[0].UserAgent.ShouldContain("cratis-synthetic-probe/1");
}
#endif
