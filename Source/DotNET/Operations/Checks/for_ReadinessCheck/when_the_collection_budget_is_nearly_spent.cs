// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_ReadinessCheck;

public class when_the_collection_budget_is_nearly_spent : a_check
{
    StubHttpHandler _handler;

    protected override IWatchdogCheck Check => new ReadinessCheck(_reader, new HttpClient(_handler), new ReadinessCheckOptions { Namespace = "core", LabelSelector = "app=core" }, _time);

    void Establish()
    {
        _handler = StubHttpHandler.Answering(HttpStatusCode.OK);
        _reader.Pods.Add(new PodInfo("core", "core-1", "Running", _time.Now.AddDays(-1), [new ContainerInfo("core")], PodIp: "10.0.0.1", Labels: new Dictionary<string, string> { ["app"] = "core" }));
    }

    async Task Because() => await Run(budget: TimeSpan.FromSeconds(10));

    [Fact] void should_not_probe_with_less_time_than_a_probe_may_take() => _handler.Requests.ShouldBeEmpty();
    [Fact] void should_say_the_pod_was_not_probed_rather_than_report_an_outage() => Keys.ShouldContainOnly("watchdog:budget:readiness:core/core-1");
}
#endif
