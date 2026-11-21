// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Immutable;
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations.for_Watchdog;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_the_cratis_infrastructure_reference_watchdog : Specification
{
    DeclaredResources _declared;
    Dictionary<string, string> _environment;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Watchdog(
            "cluster-health-watchdog",
            new WatchdogArgs
            {
                Namespace = "operations",
                Deployment = "cratis",
                Image = "cratis/showrunner-watchdog-image:1.2.3",
                WatchCertificates = true,
                Discord = new WatchdogDiscordArgs { WebhookUrl = "https://discord.example.com/api/webhooks/1/secret-webhook" },
                Smtp = new WatchdogSmtpArgs { Server = "smtp.example.com", Username = "smtp-user", Password = "smtp-password-value", FromName = "Cratis", From = "alerts@example.com", To = "ops@example.com" },
                Direct = new WatchdogDirectArgs { Url = "https://direct.example.com", Token = "direct-token-value" },
                HeartbeatUrl = "https://hc-ping.example.com/abc-secret",
                CronJobs =
                [
                    new WatchdogCronJobArgs { Namespace = "databases", Name = "db-backups", MaximumAge = "3h" },
                    new WatchdogCronJobArgs { Name = "runner-watchdog", MaximumAge = "30m", Optional = true }
                ],
                Pbm = new WatchdogPbmArgs { Namespace = "databases" },
                RestoreDrill = new WatchdogRestoreDrillArgs { Namespace = "databases-drill" },
                Readiness = new WatchdogReadinessArgs { Namespace = "core", Selector = "app=core", Container = "core" },
                PublicUrls = ["https://app.example.com/", "https://api.example.com/health"],
                VolumeAdvice = "see the storage runbook"
            }));
        _environment = ResourceJson.Environment(_declared.OfType("kubernetes:batch/v1:CronJob").Single(job => job.KubernetesName == "cluster-health-watchdog"));
    }

    [Fact] void should_run_every_check_that_was_configured() => _environment["WATCHDOG_CHECKS"].ShouldEqual("pods,nodes,volumes,certificates,cronjobs,pbm-backups,restore-drill,readiness,public-urls");
    [Fact] void should_name_the_deployment_in_alerts() => _environment["WATCHDOG_DEPLOYMENT"].ShouldEqual("cratis");
    [Fact] void should_use_the_image_it_was_given() => ((string)ResourceJson.Container(_declared.OfType("kubernetes:batch/v1:CronJob").Single(job => job.KubernetesName == "cluster-health-watchdog"))["image"]!).ShouldEqual("cratis/showrunner-watchdog-image:1.2.3");
    [Fact] void should_describe_the_cronjobs_it_expects_to_keep_succeeding() => _environment["WATCHDOG_CRONJOBS"].ShouldEqual("databases/db-backups=3h,runner-watchdog=30m?");
    [Fact] void should_say_where_the_backups_and_drill_live() => (_environment["PBM_NAMESPACE"], _environment["RESTORE_DRILL_NAMESPACE"]).ShouldEqual(("databases", "databases-drill"));
    [Fact] void should_describe_the_workload_whose_readiness_is_probed() => (_environment["READINESS_NAMESPACE"], _environment["READINESS_SELECTOR"], _environment["READINESS_CONTAINER"]).ShouldEqual(("core", "app=core", "core"));
    [Fact] void should_list_the_public_urls() => _environment["PUBLIC_URLS"].ShouldEqual("https://app.example.com/,https://api.example.com/health");
    [Fact] void should_describe_the_email_channel_except_its_credentials() => (_environment["SMTP_SERVER"], _environment["SMTP_PORT"], _environment["ALERT_TO"]).ShouldEqual(("smtp.example.com", "587", "ops@example.com"));
    [Fact] void should_take_every_credential_from_the_secret() => new[] { "DISCORD_WEBHOOK_URL", "SMTP_USERNAME", "SMTP_PASSWORD", "DIRECT_ALERTS_TOKEN", "WATCHDOG_HEARTBEAT_URL" }.All(key => _environment[key] == $"secret:cluster-health-watchdog-channels/{key}").ShouldBeTrue();
    [Fact] void should_write_every_credential_to_that_secret() => ((ImmutableDictionary<string, object>)_declared.Secret("cluster-health-watchdog-channels").Inputs["stringData"]).Keys.ShouldContainOnly("DISCORD_WEBHOOK_URL", "SMTP_USERNAME", "SMTP_PASSWORD", "DIRECT_ALERTS_TOKEN", "WATCHDOG_HEARTBEAT_URL");
    [Fact] void should_let_the_cluster_role_read_certificates() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:ClusterRole").Single()).ShouldContain("cert-manager.io/certificates:list");
    [Fact] void should_grant_a_reader_role_in_each_other_namespace_it_looks_at() => _declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Select(role => role.KubernetesNamespace).ShouldContainOnly("operations", "databases", "databases-drill");
    [Fact] void should_let_the_backups_namespace_be_read_for_cronjobs_and_pbm_backups() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Single(role => role.KubernetesNamespace == "databases")).ShouldContainOnly("batch/cronjobs:get", "psmdb.percona.com/perconaservermongodbbackups:list");
    [Fact] void should_let_the_drill_namespace_be_read_for_its_cronjob_and_jobs() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Single(role => role.KubernetesNamespace == "databases-drill")).ShouldContainOnly("batch/cronjobs:get", "batch/jobs:list");
    [Fact] void should_not_grant_anything_on_secrets() => _declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:ClusterRole").Concat(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role")).SelectMany(ResourceJson.Rules).Any(rule => rule.Contains("secrets")).ShouldBeFalse();
}
#endif
