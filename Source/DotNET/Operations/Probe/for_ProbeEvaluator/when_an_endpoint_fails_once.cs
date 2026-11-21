// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Probe.for_ProbeEvaluator;

public class when_an_endpoint_fails_once : a_probe
{
    ProbeReport _report;

    void Establish() => _status = request => request.RequestUri!.Host == "app.cratis.direct" ? (_calls["app.cratis.direct/health"] == 1 ? 503 : 200) : 401;

    async Task Because() => _report = await Evaluator.Evaluate(Targets, TimeSpan.FromSeconds(30), CancellationToken.None);

    [Fact] void should_ask_again_only_the_failing_endpoint() => _handler.Requests.Count.ShouldEqual(4);
    [Fact] void should_not_alert_for_a_flaky_endpoint() => _report.IsDown.ShouldBeFalse();
    [Fact] void should_report_it_as_flaky() => _report.Flaky.Single().Description.ShouldEqual("direct/health: 200");
    [Fact] void should_make_two_rounds() => _report.Rounds.Count.ShouldEqual(2);
    [Fact] void should_wait_the_round_gap_before_asking_again() => _time.Now.ShouldEqual(new DateTimeOffset(2026, 7, 1, 12, 0, 30, TimeSpan.Zero));
}
#endif
