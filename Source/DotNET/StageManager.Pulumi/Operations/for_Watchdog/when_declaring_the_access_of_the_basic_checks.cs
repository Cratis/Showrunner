// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations.for_Watchdog;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_the_access_of_the_basic_checks : Specification
{
    DeclaredResources _declared;

    async Task Because() => _declared = await DeclaredResources.Run(() => _ = new Watchdog("watchdog", new WatchdogArgs { Namespace = "kelda" }));

    [Fact] void should_let_the_cluster_role_read_pods_nodes_the_kubelet_summary_and_claims() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:ClusterRole").Single()).ShouldContainOnly("/pods:list", "/nodes:list", "/nodes/proxy:get", "/persistentvolumeclaims:list");
    [Fact] void should_grant_nothing_that_writes_cluster_wide() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:ClusterRole").Single()).Any(rule => rule.Contains("create") || rule.Contains("patch") || rule.Contains("update") || rule.Contains("delete")).ShouldBeFalse();
    [Fact] void should_grant_the_state_config_map_only_get_and_patch_by_name() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Single()).ShouldContain("/configmaps[watchdog-state]:get,patch");
    [Fact] void should_allow_creating_a_config_map_so_the_first_run_can_make_its_state() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Single()).ShouldContain("/configmaps:create");
    [Fact] void should_allow_listing_its_own_jobs_in_its_own_namespace_only() => ResourceJson.Rules(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").Single()).ShouldContain("batch/jobs:list");
    [Fact] void should_bind_the_cluster_role_to_the_service_account() => ResourceJson.Of(_declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:ClusterRoleBinding").Single())["subjects"]![0]!["name"]!.ToString().ShouldEqual("watchdog");
    [Fact] void should_write_no_secret_when_there_is_no_channel_credential() => _declared.Secrets.ShouldBeEmpty();
}
#endif
