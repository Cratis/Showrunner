// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorProductionSpec = Cratis.Showrunner.StageManager.Productions.ProductionSpec;
using OperatorProductionValidator = Cratis.Showrunner.StageManager.Productions.ProductionValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions.for_Production;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_tuned_managed_database_with_metrics : Specification
{
    DeclaredResources _declared;
    OperatorProductionSpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Production(
            "stage-manager-production",
            new ProductionArgs
            {
                Name = "keldakraft",
                Namespace = PulumiSpecs.Namespace,
                Version = "19.195.0",
                Database = new ProductionDatabaseArgs
                {
                    Mode = DatabaseMode.Managed,
                    Members = 3,
                    MemoryLimit = "1536Mi",
                    MemoryRequest = "1536Mi",
                    CpuRequest = "250m",
                    CpuLimit = "2",
                    WiredTigerCacheSizeRatio = 0.39,
                    TcmallocReleaseRate = 8_388_608,
                    SetParameters = { ["logLevel"] = "warn" },
                    Metrics = new DatabaseMetricsArgs { NodeSelector = { ["workload"] = "compute" } },
                },
                Vault = new ProductionVaultArgs
                {
                    Mode = VaultMode.Managed,
                    Managed = new ManagedVaultArgs
                    {
                        Tier = VaultTier.Production,
                        StorageSize = "5Gi",
                        Unseal = new VaultUnsealArgs { Mode = VaultUnsealMode.KubernetesSecret },
                    },
                },
            }));
        _spec = _declared.SpecOf<OperatorProductionSpec>("Production");
    }

    [Fact] void should_request_the_memory() => _spec.Database.MemoryRequest.ShouldEqual("1536Mi");
    [Fact] void should_request_the_cpu() => _spec.Database.CpuRequest.ShouldEqual("250m");
    [Fact] void should_limit_the_cpu() => _spec.Database.CpuLimit.ShouldEqual("2");
    [Fact] void should_size_the_cache() => _spec.Database.WiredTigerCacheSizeRatio.ShouldEqual(0.39);
    [Fact] void should_set_the_release_rate() => _spec.Database.TcmallocReleaseRate.ShouldEqual(8_388_608);
    [Fact] void should_set_further_parameters() => _spec.Database.SetParameters["logLevel"].ShouldEqual("warn");
    [Fact] void should_run_the_exporters() => _spec.Database.Metrics.Enabled.ShouldBeTrue();
    [Fact] void should_place_the_exporters() => _spec.Database.Metrics.NodeSelector["workload"].ShouldEqual("compute");
    [Fact] void should_be_valid_for_the_operator() => OperatorProductionValidator.ValidateEach(_spec).ShouldBeEmpty();
}
#endif
