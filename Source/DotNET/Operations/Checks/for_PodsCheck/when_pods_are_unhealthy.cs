// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PodsCheck;

public class when_pods_are_unhealthy : a_check
{
    protected override IWatchdogCheck Check => new PodsCheck(_reader);

    void Establish()
    {
        _reader.Pods.Add(new PodInfo("shop", "crashing", "Running", _time.Now.AddDays(-1), [new ContainerInfo("web", false, 7, "CrashLoopBackOff")]));
        _reader.Pods.Add(new PodInfo("shop", "pulling", "Pending", _time.Now.AddMinutes(-2), [new ContainerInfo("web", false, 0, "ImagePullBackOff")]));
        _reader.Pods.Add(new PodInfo("shop", "init-broken", "Pending", _time.Now.AddMinutes(-2), [new ContainerInfo("init", false, 0, "CreateContainerConfigError")]));
        _reader.Pods.Add(new PodInfo("shop", "starting", "Running", _time.Now.AddMinutes(-1), [new ContainerInfo("web", false, 0, "ContainerCreating")]));
        _reader.Pods.Add(new PodInfo("shop", "fine", "Running", _time.Now.AddDays(-1), [new ContainerInfo("web")]));
    }

    async Task Because() => await Run();

    [Fact] void should_report_a_crash_loop() => Keys.ShouldContain("pod:shop/crashing:CrashLoopBackOff");
    [Fact] void should_describe_the_container_and_its_restarts() => _findings.First(finding => finding.Key == "pod:shop/crashing:CrashLoopBackOff").Text.ShouldEqual("shop/crashing container 'web': CrashLoopBackOff (restarts: 7)");
    [Fact] void should_report_an_image_that_cannot_be_pulled() => Keys.ShouldContain("pod:shop/pulling:ImagePullBackOff");
    [Fact] void should_report_a_broken_init_container() => Keys.ShouldContain("pod:shop/init-broken:CreateContainerConfigError");
    [Fact] void should_not_report_a_container_that_is_merely_starting() => Keys.ShouldNotContain("pod:shop/starting:ContainerCreating");
    [Fact] void should_not_report_a_healthy_pod() => Keys.Any(key => key.Contains("shop/fine")).ShouldBeFalse();
    [Fact] void should_report_nothing_else() => _findings.Count.ShouldEqual(3);
}
#endif
