// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations.for_Watchdog;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_watchdog_without_a_direct_link : Specification
{
    DeclaredResources _declared;
    IReadOnlyDictionary<string, string> _environment;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Watchdog("watchdog", new WatchdogArgs { Namespace = "operations" }));
        _environment = ResourceJson.Environment(_declared.OfType("kubernetes:batch/v1:CronJob").Single(job => job.KubernetesName == "watchdog"));
    }

    [Fact] void should_set_no_link_variable() => _environment.Keys.Any(key => key.StartsWith("DIRECT_LINK_", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_grant_nothing_on_secrets() => _declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").SelectMany(ResourceJson.Rules).Any(rule => rule.Contains("secrets")).ShouldBeFalse();
    [Fact] void should_grant_nothing_on_deployments() => _declared.OfType("kubernetes:rbac.authorization.k8s.io/v1:Role").SelectMany(ResourceJson.Rules).Any(rule => rule.Contains("deployments")).ShouldBeFalse();
}
#endif
