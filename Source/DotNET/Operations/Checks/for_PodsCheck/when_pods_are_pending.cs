// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PodsCheck;

public class when_pods_are_pending : a_check
{
    protected override IWatchdogCheck Check => new PodsCheck(_reader);

    void Establish()
    {
        _reader.Pods.Add(new PodInfo("shop", "unscheduled", "Pending", _time.Now.AddMinutes(-40), []));
        _reader.Pods.Add(new PodInfo("shop", "mounting", "Pending", _time.Now.AddMinutes(-20), [new ContainerInfo("web", false, 0, "ContainerCreating")]));
        _reader.Pods.Add(new PodInfo("shop", "just-created", "Pending", _time.Now.AddMinutes(-14), []));
    }

    async Task Because() => await Run();

    [Fact] void should_report_a_pod_that_was_never_scheduled() => _findings.First(finding => finding.Key == "pod:shop/unscheduled:Pending").Text.ShouldEqual("shop/unscheduled has been Pending for 40 minutes");
    [Fact] void should_say_why_a_pod_is_stuck_when_it_is_not_just_unscheduled() => _findings.First(finding => finding.Key == "pod:shop/mounting:Pending").Text.ShouldEqual("shop/mounting has been Pending (ContainerCreating) for 20 minutes");
    [Fact] void should_give_a_new_pod_fifteen_minutes() => Keys.ShouldNotContain("pod:shop/just-created:Pending");
}
#endif
