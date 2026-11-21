// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorPerformance = Cratis.Showrunner.StageManager.Performances.Performance;
using OperatorPerformanceSpec = Cratis.Showrunner.StageManager.Performances.PerformanceSpec;
using OperatorSpecValidator = Cratis.Showrunner.StageManager.Performances.SpecValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_worker_that_shares_the_environment : Specification
{
    DeclaredResources _declared;
    OperatorPerformanceSpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Performance(
            "keldakraft-ingest",
            new PerformanceArgs
            {
                Namespace = PulumiSpecs.Namespace,
                ProductionName = "keldakraft",
                ProductionNamespace = "platform",
                Application = new ApplicationArgs
                {
                    Image = "ghcr.io/kelda/ingest:1.2.3",
                    EventStores = ["KeldaKraft"],
                    Sinks = ["reporting"],
                    Graph = true,
                    NodeSelector = { ["workload"] = "apps" },
                    Volumes = [new VolumeArgs { Name = "trust" }, new VolumeArgs { Name = "data", Kind = VolumeKind.PersistentVolumeClaim, ClaimName = "ingest-data" }],
                    VolumeMounts = [new VolumeMountArgs { Volume = "trust", Path = "/var/lib/trust", ReadOnly = true }],
                    InitContainers =
                    [
                        new InitContainerArgs
                        {
                            Name = "prepare",
                            Command = ["sh", "-c"],
                            Args = ["cp /certs/* /trust"],
                            Environment = { ["MODE"] = "copy" },
                            VolumeMounts = [new VolumeMountArgs { Volume = "trust", Path = "/trust" }],
                        },
                    ],
                    Egress = [new EgressRuleArgs { Cidr = "203.0.113.0/24", Ports = [7448] }, new EgressRuleArgs { Namespace = "platform", Ports = [27017] }],
                },
                Environment = new PerformanceEnvironmentArgs { Name = "production", ConfigMapName = "keldakraft-environment", ConfigMapKey = "production.yaml" },
                Lifetime = new LifetimeArgs { Until = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero), DataRetentionDays = 14 },
            }));
        _spec = _declared.SpecOf<OperatorPerformanceSpec>("Performance");
    }

    [Fact] void should_write_no_config_map() => _declared.ConfigMaps.ShouldBeEmpty();
    [Fact] void should_reference_the_production_in_its_namespace() => (_spec.ProductionRef!.Name, _spec.ProductionRef.Namespace).ShouldEqual(("keldakraft", "platform"));
    [Fact] void should_name_the_environment_key() => _spec.Environment.ConfigMapRef.Key.ShouldEqual("production.yaml");
    [Fact] void should_use_the_sink() => _spec.Application.Sinks.ShouldContainOnly("reporting");
    [Fact] void should_read_the_graph() => _spec.Application.Graph.ShouldBeTrue();
    [Fact] void should_select_the_nodes() => _spec.Application.NodeSelector["workload"].ShouldEqual("apps");
    [Fact] void should_declare_the_volumes() => _spec.Application.Volumes.Select(volume => (volume.Name, volume.Kind, volume.ClaimName)).ShouldContainOnly(("trust", "EmptyDir", string.Empty), ("data", "PersistentVolumeClaim", "ingest-data"));
    [Fact] void should_mount_the_volume_read_only() => (_spec.Application.VolumeMounts.Single().Path, _spec.Application.VolumeMounts.Single().ReadOnly).ShouldEqual(("/var/lib/trust", true));
    [Fact] void should_run_the_init_container() => (_spec.Application.InitContainers.Single().Name, _spec.Application.InitContainers.Single().Environment["MODE"]).ShouldEqual(("prepare", "copy"));
    [Fact] void should_allow_the_address_range_and_the_namespace() => _spec.Application.Egress.Select(rule => (rule.Cidr, rule.Namespace)).ShouldContainOnly(("203.0.113.0/24", string.Empty), (string.Empty, "platform"));
    [Fact] void should_expire() => _spec.Lifetime!.Until.ShouldEqual(new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero));
    [Fact] void should_retain_the_data() => _spec.Lifetime!.DataRetentionDays.ShouldEqual(14);
    [Fact] void should_be_valid_for_the_operator() => OperatorSpecValidator.Validate(new OperatorPerformance { Metadata = new() { Name = "keldakraft-ingest", NamespaceProperty = PulumiSpecs.Namespace }, Spec = _spec }).ShouldBeEmpty();
}
#endif
