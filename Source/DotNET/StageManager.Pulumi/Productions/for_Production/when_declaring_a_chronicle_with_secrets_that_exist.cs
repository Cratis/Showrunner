// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorProductionSpec = Cratis.Showrunner.StageManager.Productions.ProductionSpec;
using OperatorProductionValidator = Cratis.Showrunner.StageManager.Productions.ProductionValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions.for_Production;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_chronicle_with_secrets_that_exist : Specification
{
    DeclaredResources _declared;
    OperatorProductionSpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Production(
            "production",
            new ProductionArgs
            {
                Namespace = PulumiSpecs.Namespace,
                Version = "19.164.2",
                Database = new ProductionDatabaseArgs { ConnectionSecretName = "existing-database" },
                Vault = new ProductionVaultArgs
                {
                    Mode = VaultMode.External,
                    Address = "https://vault.example.com:8200",
                    TokenSecretName = "existing-vault",
                    TokenSecretKey = "root",
                },
                Graph = new ProductionGraphArgs { Mode = GraphMode.External, CredentialsSecretName = "existing-graph", Edition = GraphEdition.Enterprise },
                Sinks = new ProductionSinksArgs
                {
                    ReadModels = "reporting",
                    Configurations =
                    [
                        new SinkArgs { Name = "reporting", Kind = SinkKind.PostgreSql, ConnectionSecret = new SecretReferenceArgs { Name = "existing-reporting" } },
                    ],
                },
            }));
        _spec = _declared.SpecOf<OperatorProductionSpec>("Production");
    }

    [Fact] void should_name_the_production_after_the_component() => _declared.Custom("Production").KubernetesName.ShouldEqual("production");
    [Fact] void should_default_to_one_silo() => _spec.Replicas.ShouldEqual(1);
    [Fact] void should_write_no_secret() => _declared.Secrets.ShouldBeEmpty();
    [Fact] void should_name_the_existing_database_secret() => _spec.Database.ConnectionSecretName.ShouldEqual("existing-database");
    [Fact] void should_name_the_existing_vault_token_and_key() => (_spec.Vault.TokenSecretRef!.Name, _spec.Vault.TokenSecretRef.Key).ShouldEqual(("existing-vault", "root"));
    [Fact] void should_use_an_external_graph() => (_spec.Graph.Mode, _spec.Graph.Edition, _spec.Graph.CredentialsSecretName).ShouldEqual(("External", "Enterprise", "existing-graph"));
    [Fact] void should_configure_the_sink() => (_spec.Sinks.ReadModels, _spec.Sinks.Configurations[0].Kind, _spec.Sinks.Configurations[0].ConnectionSecretRef!.Name).ShouldEqual(("reporting", "PostgreSql", "existing-reporting"));
    [Fact] void should_be_valid_for_the_operator() => OperatorProductionValidator.Validate(_spec).ShouldBeEmpty();
}
#endif
