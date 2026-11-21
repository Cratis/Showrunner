// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations.for_Watchdog;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_every_job_kind_and_check : Specification
{
    static readonly string[] _kinds = ["runners", "agent-harness", "volumes", "probe"];

    DeclaredResources _declared;

    RecordedResource Resource(string type, string name) => _declared.OfType(type).Single(resource => resource.Name == name);

    RecordedResource Job(string kind) => Resource("kubernetes:batch/v1:CronJob", $"watchdog-{kind}-cronjob");

    Dictionary<string, string> Environment(string kind) => ResourceJson.Environment(Job(kind));

    List<string> Rules(string type, string name) => ResourceJson.Rules(Resource(type, name));

    async Task Because() => _declared = await DeclaredResources.Run(() => _ = new Watchdog(
        "watchdog",
        new WatchdogArgs
        {
            Namespace = "operations",
            Deployment = "cratis",
            TestAlertJob = false,
            Discord = new WatchdogDiscordArgs { WebhookUrl = "https://discord.example.com/api/webhooks/1/secret-webhook" },
            Direct = new WatchdogDirectArgs { Url = "https://direct.example.com", Token = "direct-token-value" },
            Prometheus = new WatchdogPrometheusArgs { Url = "http://prometheus-server.infrastructure:80", PlatformNamespace = "platform", SeriesThreshold = 400000 },
            Chronicle = new WatchdogChronicleArgs { Url = "https://chronicle:8080", ClientId = "watchdog", ClientSecret = "chronicle-secret-value", CertificateAuthority = "ca-pem-value" },
            GitHubAudit = new WatchdogGitHubAuditArgs { Token = "github-token-value", Organization = "Acme" },
            Runners = new WatchdogRunnersArgs { Namespace = "arc-runners", ScaleSets = ["big", "small"], MaximumJobMinutes = 60, PendingMinutes = 20 },
            AgentHarness = new WatchdogAgentHarnessArgs { Namespace = "direct", CleanupEnabled = true },
            VolumeSweeper = new WatchdogVolumeSweeperArgs { Username = "upcloud-user", Password = "upcloud-password-value", DryRun = true, MaximumPerRun = 4 },
            Probe = new WatchdogProbeArgs
            {
                RoundGapSeconds = 5,
                Targets = [new WatchdogProbeTargetArgs { Name = "direct", BaseUrl = "https://app.example.com", Checks = [new WatchdogProbeCheckArgs { Id = "health", Path = "/health", ExpectStatus = 200 }] }]
            }
        }));

    [Fact] void should_run_the_metrics_and_budget_checks_when_prometheus_is_named() => ResourceJson.Environment(Resource("kubernetes:batch/v1:CronJob", "watchdog-cronjob"))["WATCHDOG_CHECKS"].ShouldContain("metrics,telemetry-budget");
    [Fact] void should_run_the_chronicle_and_audit_checks_when_they_are_named() => ResourceJson.Environment(Resource("kubernetes:batch/v1:CronJob", "watchdog-cronjob"))["WATCHDOG_CHECKS"].ShouldContain("chronicle-observers,actions-audit");
    [Fact] void should_not_run_a_job_kind_in_the_cluster_pass() => ResourceJson.Environment(Resource("kubernetes:batch/v1:CronJob", "watchdog-cronjob"))["WATCHDOG_CHECKS"].ShouldNotContain("runners");
    [Fact] void should_pass_prometheus_settings() => (ResourceJson.Environment(Resource("kubernetes:batch/v1:CronJob", "watchdog-cronjob"))["PROMETHEUS_URL"], ResourceJson.Environment(Resource("kubernetes:batch/v1:CronJob", "watchdog-cronjob"))["PLATFORM_NAMESPACE"], ResourceJson.Environment(Resource("kubernetes:batch/v1:CronJob", "watchdog-cronjob"))["PROMETHEUS_SERIES_THRESHOLD"]).ShouldEqual(("http://prometheus-server.infrastructure:80", "platform", "400000"));
    [Fact] void should_take_the_chronicle_secret_from_a_secret() => ResourceJson.Environment(Resource("kubernetes:batch/v1:CronJob", "watchdog-cronjob"))["CHRONICLE_CLIENT_SECRET"].ShouldEqual("secret:watchdog-channels/CHRONICLE_CLIENT_SECRET");
    [Fact] void should_take_the_github_token_from_a_secret() => ResourceJson.Environment(Resource("kubernetes:batch/v1:CronJob", "watchdog-cronjob"))["GITHUB_AUDIT_TOKEN"].ShouldEqual("secret:watchdog-channels/GITHUB_AUDIT_TOKEN");
    [Fact] void should_declare_a_cronjob_per_job_kind() => _declared.OfType("kubernetes:batch/v1:CronJob").Select(job => job.Name).ShouldContainOnly("watchdog-cronjob", "watchdog-runners-cronjob", "watchdog-agent-harness-cronjob", "watchdog-volumes-cronjob", "watchdog-probe-cronjob");
    [Fact] void should_run_each_job_kind_with_its_own_command() => _kinds.Select(kind => (string)ResourceJson.Container(Job(kind))["args"]![0]!).ShouldContainOnly(_kinds);
    [Fact] void should_run_each_job_kind_as_its_own_service_account() => _kinds.Select(kind => (string)ResourceJson.Of(Job(kind))["spec"]!["jobTemplate"]!["spec"]!["template"]!["spec"]!["serviceAccountName"]!).ShouldContainOnly(_kinds.Select(kind => $"watchdog-{kind}").ToArray());
    [Fact] void should_let_the_runners_job_only_list_and_delete_runners_in_the_runner_namespace() => Rules("kubernetes:rbac.authorization.k8s.io/v1:Role", "watchdog-runners-arc-runners-role").ShouldContainOnly("actions.github.com/ephemeralrunners:list,delete");
    [Fact] void should_tell_the_runners_job_which_scale_sets_and_limits() => (Environment("runners")["RUNNER_SCALE_SETS"], Environment("runners")["MAX_JOB_MINUTES"], Environment("runners")["PENDING_MINUTES"]).ShouldEqual(("big,small", "60", "20"));
    [Fact] void should_keep_the_runners_state_in_its_own_config_map() => Environment("runners")["RUNNER_STATE_CONFIGMAP"].ShouldEqual("watchdog-runners-state");
    [Fact] void should_let_the_runners_job_write_only_its_own_state() => Rules("kubernetes:rbac.authorization.k8s.io/v1:Role", "watchdog-runners-role").ShouldContain("/configmaps[watchdog-state-runners,watchdog-runners-state]:get,patch");
    [Fact] void should_let_the_agent_job_delete_jobs_and_read_pods_and_secrets_in_the_agent_namespace() => Rules("kubernetes:rbac.authorization.k8s.io/v1:Role", "watchdog-agent-harness-direct-role").ShouldContainOnly("/pods:list", "/events:list", "/secrets:get", "batch/jobs:get,list,delete");
    [Fact] void should_enable_agent_cleanup_only_when_asked() => Environment("agent-harness")["CLEANUP_ENABLED"].ShouldEqual("true");
    [Fact] void should_let_the_sweeper_only_list_persistent_volumes() => Rules("kubernetes:rbac.authorization.k8s.io/v1:ClusterRole", "watchdog-volumes-cluster-role").ShouldContainOnly("/persistentvolumes:list");
    [Fact] void should_keep_the_cloud_credentials_in_a_secret_of_the_sweeper() => (Environment("volumes")["UPCLOUD_USERNAME"], Environment("volumes")["UPCLOUD_PASSWORD"]).ShouldEqual(("secret:watchdog-volumes-credentials/UPCLOUD_USERNAME", "secret:watchdog-volumes-credentials/UPCLOUD_PASSWORD"));
    [Fact] void should_pass_the_sweeper_limits() => (Environment("volumes")["DRY_RUN"], Environment("volumes")["MAX_PER_RUN"]).ShouldEqual(("true", "4"));
    [Fact] void should_pass_the_probe_targets_as_json() => Environment("probe")["PROBE_TARGETS"].ShouldContain("\"baseUrl\":\"https://app.example.com\"");
    [Fact] void should_pass_the_probe_round_gap() => Environment("probe")["PROBE_ROUND_GAP_SECONDS"].ShouldEqual("5");
    [Fact] void should_give_the_probe_no_cluster_rights() => _declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:ClusterRole").Select(role => role.Name).ShouldNotContain("watchdog-probe-cluster-role");
    [Fact] void should_give_every_job_the_same_direct_channel_as_the_cluster_pass() => _kinds.All(kind => Environment(kind)["DIRECT_ALERTS_URL"] == "https://direct.example.com").ShouldBeTrue();
    [Fact] void should_hand_every_job_the_discord_webhook_from_the_secret() => _kinds.All(kind => Environment(kind)["DISCORD_WEBHOOK_URL"] == "secret:watchdog-channels/DISCORD_WEBHOOK_URL").ShouldBeTrue();
    [Fact] void should_not_hand_a_job_a_credential_it_has_no_use_for() => _kinds.Any(kind => Environment(kind).ContainsKey("CHRONICLE_CLIENT_SECRET") || Environment(kind).ContainsKey("GITHUB_AUDIT_TOKEN")).ShouldBeFalse();
    [Fact] void should_never_leave_a_secret_value_in_a_cronjob() => _declared.OfType("kubernetes:batch/v1:CronJob").Any(job => ResourceJson.Of(job).ToJsonString().Contains("upcloud-password-value") || ResourceJson.Of(job).ToJsonString().Contains("chronicle-secret-value")).ShouldBeFalse();
}
#endif
