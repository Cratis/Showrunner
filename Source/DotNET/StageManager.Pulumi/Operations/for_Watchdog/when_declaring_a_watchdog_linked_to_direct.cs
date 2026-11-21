// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Immutable;
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations.for_Watchdog;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_watchdog_linked_to_direct : Specification
{
    DeclaredResources _declared;
    IReadOnlyDictionary<string, string> _environment;
    IReadOnlyList<string> _ownRules;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Watchdog(
            "watchdog",
            new WatchdogArgs
            {
                Namespace = "operations",
                Direct = new WatchdogDirectArgs { Url = "https://direct.example.com", Token = "alert-token" },
                DirectLink = new WatchdogDirectLinkArgs
                {
                    Url = "https://direct.example.com",
                    InstanceName = "showrunner-prod",
                    EnrollmentToken = "enrollment-token",
                    Resource = "7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c",
                    OutboxLimit = 300,
                    Remedies =
                    [
                        new WatchdogRemedyArgs { Remedy = "restart-web", Namespace = "shop", Deployment = "web" },
                        new WatchdogRemedyArgs { Remedy = "restart-api", Namespace = "shop", Deployment = "api" }
                    ]
                }
            }));
        _environment = ResourceJson.Environment(_declared.OfType("kubernetes:batch/v1:CronJob").Single(job => job.KubernetesName == "watchdog"));
        _ownRules = ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Single(role => role.KubernetesNamespace == "operations"));
    }

    [Fact] void should_describe_the_link_in_the_environment() => (_environment["DIRECT_LINK_URL"], _environment["DIRECT_LINK_INSTANCE"], _environment["DIRECT_LINK_RESOURCE"], _environment["DIRECT_LINK_OUTBOX_LIMIT"]).ShouldEqual(("https://direct.example.com", "showrunner-prod", "7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c", "300"));
    [Fact] void should_name_the_state_the_link_keeps() => _environment["DIRECT_LINK_STATE_NAME"].ShouldEqual("watchdog-direct-link");
    [Fact] void should_bind_each_remedy_to_its_deployment() => _environment["DIRECT_LINK_REMEDIES"].ShouldEqual("restart-web=shop/web,restart-api=shop/api");
    [Fact] void should_take_the_enrollment_token_from_the_secret() => _environment["DIRECT_LINK_ENROLLMENT_TOKEN"].ShouldEqual("secret:watchdog-channels/DIRECT_LINK_ENROLLMENT_TOKEN");
    [Fact] void should_write_the_enrollment_token_next_to_the_other_credentials() => ((ImmutableDictionary<string, object>)_declared.Secret("watchdog-channels").Inputs["stringData"]).Keys.ShouldContainOnly("DIRECT_ALERTS_TOKEN", "DIRECT_LINK_ENROLLMENT_TOKEN");
    [Fact] void should_keep_the_alert_webhook_channel() => (_environment["DIRECT_ALERTS_URL"], _environment["DIRECT_ALERTS_TOKEN"]).ShouldEqual(("https://direct.example.com", "secret:watchdog-channels/DIRECT_ALERTS_TOKEN"));
    [Fact] void should_not_enable_http_remedies_unless_asked() => _environment.ContainsKey("DIRECT_LINK_HTTP_REMEDIES").ShouldBeFalse();
    [Fact] void should_let_it_read_and_patch_its_link_state_config_map_by_name() => _ownRules.ShouldContain("/configmaps[watchdog-state,watchdog-direct-link]:get,patch");
    [Fact] void should_let_it_read_and_patch_its_link_secret_by_name() => _ownRules.ShouldContain("/secrets[watchdog-direct-link]:get,patch");
    [Fact] void should_let_it_create_the_link_secret_once() => _ownRules.ShouldContain("/secrets:create");
    [Fact] void should_grant_get_and_patch_on_exactly_the_bound_deployments() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Single(role => role.KubernetesNamespace == "shop")).ShouldContainOnly("apps/deployments[web,api]:get,patch");
    [Fact] void should_grant_nothing_destructive_to_the_deployments() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Single(role => role.KubernetesNamespace == "shop")).Any(rule => rule.Contains("delete") || rule.Contains("create") || rule.Contains("update")).ShouldBeFalse();
}
#endif
