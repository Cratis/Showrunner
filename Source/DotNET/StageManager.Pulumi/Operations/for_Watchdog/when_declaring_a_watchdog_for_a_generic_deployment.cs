// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations.for_Watchdog;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_watchdog_for_a_generic_deployment : Specification
{
    DeclaredResources _declared;
    RecordedResource _cronJob;
    RecordedResource _testAlert;
    Dictionary<string, string> _environment;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Watchdog(
            "watchdog",
            new WatchdogArgs
            {
                Namespace = "kelda",
                Direct = new WatchdogDirectArgs { Url = "https://direct.example.com", Token = "direct-token-value", Source = "kelda" }
            }));
        _cronJob = _declared.OfType("kubernetes:batch/v1:CronJob").Single(job => job.KubernetesName == "watchdog");
        _testAlert = _declared.OfType("kubernetes:batch/v1:CronJob").Single(job => job.KubernetesName == "watchdog-test-alert");
        _environment = ResourceJson.Environment(_cronJob);
    }

    [Fact] void should_run_every_five_minutes() => ((string)ResourceJson.Of(_cronJob)["spec"]!["schedule"]!).ShouldEqual("*/5 * * * *");
    [Fact] void should_never_run_two_passes_at_once() => ((string)ResourceJson.Of(_cronJob)["spec"]!["concurrencyPolicy"]!).ShouldEqual("Forbid");
    [Fact] void should_be_killed_after_the_deadline_the_watchdog_budgets_for() => ((int)ResourceJson.Of(_cronJob)["spec"]!["jobTemplate"]!["spec"]!["activeDeadlineSeconds"]!).ShouldEqual(240);
    [Fact] void should_tell_the_watchdog_the_same_deadline() => _environment["DEADLINE_SECONDS"].ShouldEqual("240");
    [Fact] void should_not_retry_a_failed_run_because_the_next_run_reports_it() => ((int)ResourceJson.Of(_cronJob)["spec"]!["jobTemplate"]!["spec"]!["backoffLimit"]!).ShouldEqual(0);
    [Fact] void should_keep_failed_jobs_for_the_next_run_to_report() => ((int)ResourceJson.Of(_cronJob)["spec"]!["failedJobsHistoryLimit"]!).ShouldEqual(3);
    [Fact] void should_run_the_cluster_pass() => ResourceJson.Container(_cronJob)["args"]!.AsArray().Select(argument => (string)argument!).ShouldContainOnly("cluster");
    [Fact] void should_use_the_published_image() => ((string)ResourceJson.Container(_cronJob)["image"]!).ShouldEqual("cratis/showrunner-watchdog-image:latest");
    [Fact] void should_run_the_basic_checks_only() => _environment["WATCHDOG_CHECKS"].ShouldEqual("pods,nodes,volumes");
    [Fact] void should_tell_the_watchdog_which_cronjob_it_is_to_detect_its_own_failed_jobs() => _environment["WATCHDOG_CRONJOB"].ShouldEqual("watchdog");
    [Fact] void should_tell_the_watchdog_its_schedule_to_detect_missed_runs() => _environment["WATCHDOG_SCHEDULE"].ShouldEqual("*/5 * * * *");
    [Fact] void should_name_the_state_config_map() => _environment["WATCHDOG_STATE_CONFIGMAP"].ShouldEqual("watchdog-state");
    [Fact] void should_name_the_namespace() => _environment["WATCHDOG_NAMESPACE"].ShouldEqual("kelda");
    [Fact] void should_name_the_job_for_the_downward_api() => _environment["WATCHDOG_JOB_NAME"].ShouldEqual("field:metadata.labels['batch.kubernetes.io/job-name']");
    [Fact] void should_configure_direct() => (_environment["DIRECT_ALERTS_URL"], _environment["DIRECT_ALERTS_SOURCE"]).ShouldEqual(("https://direct.example.com", "kelda"));
    [Fact] void should_take_the_direct_token_from_a_secret() => _environment["DIRECT_ALERTS_TOKEN"].ShouldEqual("secret:watchdog-channels/DIRECT_ALERTS_TOKEN");
    [Fact] void should_not_configure_a_channel_it_was_not_given() => _environment.Keys.Any(key => key is "DISCORD_WEBHOOK_URL" or "SMTP_SERVER" or "WATCHDOG_HEARTBEAT_URL").ShouldBeFalse();
    [Fact] void should_not_put_the_token_in_the_job_spec() => ResourceJson.Of(_cronJob).ToJsonString().ShouldNotContain("direct-token-value");
    [Fact] void should_keep_the_token_in_the_secret() => _declared.StringData("watchdog-channels", "DIRECT_ALERTS_TOKEN").ShouldNotBeNull();
    [Fact] void should_not_write_secrets_for_channels_it_was_not_given() => ((System.Collections.Immutable.ImmutableDictionary<string, object>)_declared.Secret("watchdog-channels").Inputs["stringData"]).Keys.ShouldContainOnly("DIRECT_ALERTS_TOKEN");
    [Fact] void should_run_as_its_own_service_account() => ((string)ResourceJson.Of(_cronJob)["spec"]!["jobTemplate"]!["spec"]!["template"]!["spec"]!["serviceAccountName"]!).ShouldEqual("watchdog");
    [Fact] void should_not_allow_privilege_escalation() => ((bool)ResourceJson.Container(_cronJob)["securityContext"]!["allowPrivilegeEscalation"]!).ShouldBeFalse();
    [Fact] void should_use_a_read_only_root_file_system() => ((bool)ResourceJson.Container(_cronJob)["securityContext"]!["readOnlyRootFilesystem"]!).ShouldBeTrue();
    [Fact] void should_drop_every_capability() => ((string)ResourceJson.Container(_cronJob)["securityContext"]!["capabilities"]!["drop"]![0]!).ShouldEqual("ALL");
    [Fact] void should_not_run_as_root() => ((bool)ResourceJson.Of(_cronJob)["spec"]!["jobTemplate"]!["spec"]!["template"]!["spec"]!["securityContext"]!["runAsNonRoot"]!).ShouldBeTrue();
    [Fact] void should_declare_a_suspended_cronjob_for_the_delivery_test() => ((bool)ResourceJson.Of(_testAlert)["spec"]!["suspend"]!).ShouldBeTrue();
    [Fact] void should_run_the_delivery_test_command_in_it() => ResourceJson.Container(_testAlert)["args"]!.AsArray().Select(argument => (string)argument!).ShouldContainOnly("test-alert");
    [Fact] void should_deliver_the_test_to_the_same_channels() => ResourceJson.Environment(_testAlert)["DIRECT_ALERTS_TOKEN"].ShouldEqual("secret:watchdog-channels/DIRECT_ALERTS_TOKEN");
    [Fact] void should_declare_no_role_in_another_namespace() => _declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Count.ShouldEqual(1);
}
#endif
