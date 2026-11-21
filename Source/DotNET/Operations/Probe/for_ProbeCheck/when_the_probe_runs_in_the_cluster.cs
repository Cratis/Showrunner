// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Probe.for_ProbeCheck;

public class when_the_probe_runs_in_the_cluster : a_probe
{
    IReadOnlyCollection<Finding> _findings;
    IReadOnlyCollection<Finding> _flaky;

    async Task Because()
    {
        _status = request => request.RequestUri!.Host == "app.cratis.direct" ? 503 : 401;
        var check = new ProbeCheck(Evaluator, Targets, TimeSpan.Zero);
        _findings = await check.Collect(CheckContexts.At(_time, null, null), CancellationToken.None);

        _calls.Clear();
        _status = request => request.RequestUri!.Host == "app.cratis.direct" ? (_calls.GetValueOrDefault("app.cratis.direct/health") == 1 ? 503 : 200) : 401;
        _flaky = await check.Collect(CheckContexts.At(_time, null, null), CancellationToken.None);
    }

    [Fact] void should_make_a_finding_of_an_endpoint_that_is_down() => _findings.Single().Key.ShouldEqual("probe:direct/health");
    [Fact] void should_describe_it_for_a_human() => _findings.Single().Text.ShouldEqual("public endpoint direct/health: 503");
    [Fact] void should_not_make_a_finding_of_a_flaky_endpoint() => _flaky.ShouldBeEmpty();
}
#endif
