// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PodsCheck;

public class when_pods_were_evicted : a_check
{
    protected override IWatchdogCheck Check => new PodsCheck(_reader);

    void Establish()
    {
        // The previous run started five minutes ago.
        _reader.Pods.Add(new PodInfo("a", "recent", "Failed", _time.Now.AddDays(-9), [], "Evicted", DisruptedAt: _time.Now.AddMinutes(-2)));
        _reader.Pods.Add(new PodInfo("b", "recent-by-container", "Failed", _time.Now.AddDays(-9), [new ContainerInfo("web", TerminatedAt: _time.Now.AddMinutes(-1))], "Evicted"));
        _reader.Pods.Add(new PodInfo("c", "history", "Failed", _time.Now.AddDays(-9), [], "Evicted", DisruptedAt: _time.Now.AddHours(-3)));
        _reader.Pods.Add(new PodInfo("d", "unknown-time", "Failed", _time.Now.AddDays(-9), [], "Evicted"));
    }

    async Task Because() => await Run();

    [Fact] void should_report_an_eviction_in_the_check_window() => Keys.ShouldContain("pod:a/recent:Evicted");
    [Fact] void should_use_container_termination_when_there_is_no_disruption_condition() => Keys.ShouldContain("pod:b/recent-by-container:Evicted");
    [Fact] void should_not_report_eviction_history() => Keys.ShouldNotContain("pod:c/history:Evicted");
    [Fact] void should_report_an_eviction_whose_time_is_unknown() => Keys.ShouldContain("pod:d/unknown-time:Evicted");
    [Fact] void should_say_that_the_time_is_unavailable() => _findings.First(finding => finding.Key == "pod:d/unknown-time:Evicted").Text.ShouldContain("(eviction time unavailable)");
}
#endif
