// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations.for_Watchdog;

[Collection(PulumiSpecs.Collection)]
public class when_choosing_only_some_checks : Specification
{
    DeclaredResources _declared;
    Dictionary<string, string> _environment;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Watchdog(
            "watchdog",
            new WatchdogArgs
            {
                Namespace = "kelda",
                Checks = ["pods", "nodes", "pods"],
                Schedule = "*/10 * * * *",
                DeadlineSeconds = 120,
                RealertInterval = "12h",
                EscalateAfter = "2d",
                TestAlertJob = false,
                Labels = new Dictionary<string, string> { ["team"] = "ops" }
            }));
        _environment = ResourceJson.Environment(_declared.OfType("kubernetes:batch/v1:CronJob").Single());
    }

    [Fact] void should_run_exactly_the_checks_named_once() => _environment["WATCHDOG_CHECKS"].ShouldEqual("pods,nodes");
    [Fact] void should_grant_only_what_those_checks_read() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:ClusterRole").Single()).ShouldContainOnly("/pods:list", "/nodes:list");
    [Fact] void should_not_read_the_kubelet_when_the_volumes_check_is_off() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:ClusterRole").Single()).Any(rule => rule.Contains("proxy")).ShouldBeFalse();
    [Fact] void should_use_the_schedule_it_was_given() => ((string)ResourceJson.Of(_declared.OfType("kubernetes:batch/v1:CronJob").Single())["spec"]!["schedule"]!).ShouldEqual("*/10 * * * *");
    [Fact] void should_use_the_deadline_it_was_given_for_both_the_job_and_the_watchdog() => (((int)ResourceJson.Of(_declared.OfType("kubernetes:batch/v1:CronJob").Single())["spec"]!["jobTemplate"]!["spec"]!["activeDeadlineSeconds"]!), _environment["DEADLINE_SECONDS"]).ShouldEqual((120, "120"));
    [Fact] void should_pass_the_realert_and_escalation_intervals() => (_environment["REALERT_SECONDS"], _environment["ESCALATE_AFTER_SECONDS"]).ShouldEqual(("12h", "2d"));
    [Fact] void should_declare_no_delivery_test_cronjob_when_asked_not_to() => _declared.OfType("kubernetes:batch/v1:CronJob").Count.ShouldEqual(1);
    [Fact] void should_label_the_resources() => ((string)ResourceJson.Of(_declared.OfType("kubernetes:batch/v1:CronJob").Single())["metadata"]!["labels"]!["team"]!).ShouldEqual("ops");
}
#endif
