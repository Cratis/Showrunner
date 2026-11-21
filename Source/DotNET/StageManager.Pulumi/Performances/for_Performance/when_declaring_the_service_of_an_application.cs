// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorPerformance = Cratis.Showrunner.StageManager.Performances.Performance;
using OperatorPerformanceSpec = Cratis.Showrunner.StageManager.Performances.PerformanceSpec;
using OperatorSpecValidator = Cratis.Showrunner.StageManager.Performances.SpecValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_the_service_of_an_application : Specification
{
    DeclaredResources _declared;
    OperatorPerformanceSpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Performance(
            "keldakraft",
            new PerformanceArgs
            {
                Namespace = PulumiSpecs.Namespace,
                ProductionName = "keldakraft",
                Application = new ApplicationArgs
                {
                    Image = "ghcr.io/kelda/keldakraft:1.2.3",
                    EventStores = ["KeldaKraft"],
                    Port = 8080,
                    Replicas = 1,
                    Strategy = RolloutStrategy.Recreate,
                    ImagePullSecrets = ["registry-credentials"],
                    Probes = new ApplicationProbesArgs { StartupPath = "/health", LivenessPath = "/health", ReadinessPath = "/health" },
                    Resources = new ApplicationResourcesArgs
                    {
                        Requests = { ["cpu"] = "250m", ["memory"] = "512Mi" },
                        Limits = { ["cpu"] = "1", ["memory"] = "1Gi" },
                    },
                    Egress = [new EgressRuleArgs { Ports = [587, 80, 7448, 443] }],
                },
                Environment = new PerformanceEnvironmentArgs
                {
                    Name = "production",
                    ConfigMapName = "keldakraft-environment",
                    Content = "secrets: {}\n",
                },
            }));
        _spec = _declared.SpecOf<OperatorPerformanceSpec>("Performance");
    }

    [Fact] void should_declare_the_performance_in_the_namespace() => _declared.Custom("Performance").KubernetesNamespace.ShouldEqual(PulumiSpecs.Namespace);
    [Fact] void should_name_it_after_the_component() => _declared.Custom("Performance").KubernetesName.ShouldEqual("keldakraft");
    [Fact] void should_run_against_the_production() => _spec.ProductionRef!.Name.ShouldEqual("keldakraft");
    [Fact] void should_run_the_image() => _spec.Application.Image.ShouldEqual("ghcr.io/kelda/keldakraft:1.2.3");
    [Fact] void should_use_the_event_store() => _spec.Application.EventStores.ShouldContainOnly("KeldaKraft");
    [Fact] void should_recreate_on_rollout() => _spec.Application.Strategy.ShouldEqual("Recreate");
    [Fact] void should_pull_with_the_registry_credentials() => _spec.Application.ImagePullSecrets.ShouldContainOnly("registry-credentials");
    [Fact] void should_check_the_health_path() => (_spec.Application.Probes.StartupPath, _spec.Application.Probes.LivenessPath, _spec.Application.Probes.ReadinessPath).ShouldEqual(("/health", "/health", "/health"));
    [Fact] void should_request_the_resources() => (_spec.Application.Resources!.Requests["cpu"], _spec.Application.Resources.Requests["memory"]).ShouldEqual(("250m", "512Mi"));
    [Fact] void should_limit_the_resources() => (_spec.Application.Resources!.Limits["cpu"], _spec.Application.Resources.Limits["memory"]).ShouldEqual(("1", "1Gi"));
    [Fact] void should_open_the_egress_ports() => _spec.Application.Egress.Single().Ports.ShouldContainOnly(587, 80, 7448, 443);
    [Fact] void should_name_the_environment() => (_spec.Environment.Name, _spec.Environment.ConfigMapRef.Name).ShouldEqual(("production", "keldakraft-environment"));
    [Fact] void should_leave_the_environment_key_to_the_operator() => _spec.Environment.ConfigMapRef.Key.ShouldBeNull();
    [Fact] void should_be_valid_for_the_operator() => OperatorSpecValidator.Validate(new OperatorPerformance { Metadata = new() { Name = "keldakraft", NamespaceProperty = PulumiSpecs.Namespace }, Spec = _spec }).ShouldBeEmpty();
    [Fact] void should_write_the_environment_file_to_a_config_map() => _declared.ConfigMaps.Single().KubernetesName.ShouldEqual("keldakraft-environment");
}
#endif
