// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorProductionSpec = Cratis.Showrunner.StageManager.Productions.ProductionSpec;
using OperatorProductionValidator = Cratis.Showrunner.StageManager.Productions.ProductionValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions.for_Production;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_production_on_dedicated_nodes : Specification
{
    DeclaredResources _declared;
    OperatorProductionSpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Production(
            "stage-manager-production",
            new ProductionArgs
            {
                Name = "ada",
                Namespace = PulumiSpecs.Namespace,
                Version = "19.164.2",
                Database = new ProductionDatabaseArgs { Mode = DatabaseMode.Managed, Members = 3 },
                Vault = new ProductionVaultArgs
                {
                    Mode = VaultMode.Managed,
                    MigrateFromDefaultStorage = true,
                    Managed = new ManagedVaultArgs { Tier = VaultTier.Production, StorageSize = "5Gi", Unseal = new VaultUnsealArgs { Mode = VaultUnsealMode.KubernetesSecret } },
                },
                Scheduling = new ProductionSchedulingArgs
                {
                    NodeSelector = { ["workload"] = "data" },
                    Tolerations = [new ProductionTolerationArgs { Key = "workload", Value = "data", Effect = "NoSchedule" }],
                },
                Resources = new ProductionResourcesArgs
                {
                    Requests = { ["memory"] = "1Gi" },
                    Limits = { ["memory"] = "1536Mi" },
                },
            }));
        _spec = _declared.SpecOf<OperatorProductionSpec>("Production");
    }

    [Fact] void should_select_the_nodes() => _spec.Scheduling.NodeSelector["workload"].ShouldEqual("data");
    [Fact] void should_tolerate_the_taint() => (_spec.Scheduling.Tolerations.Single().Key, _spec.Scheduling.Tolerations.Single().Effect).ShouldEqual(("workload", "NoSchedule"));
    [Fact] void should_size_chronicle() => _spec.Resources.Limits["memory"].ShouldEqual("1536Mi");
    [Fact] void should_migrate_the_keys_of_the_database() => _spec.Vault.MigrateFromDefaultStorage.ShouldBeTrue();
    [Fact] void should_be_valid_for_the_operator() => OperatorProductionValidator.Validate(_spec).ShouldBeEmpty();
}
#endif
