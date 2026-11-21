// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Immutable;
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorProductionSpec = Cratis.Showrunner.StageManager.Productions.ProductionSpec;
using OperatorProductionValidator = Cratis.Showrunner.StageManager.Productions.ProductionValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions.for_Production;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_chronicle_next_to_an_existing_database_and_vault : Specification
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
                Version = "19.164.2",
                Replicas = 2,
                Labels = { ["environment"] = "production", ["managed-by"] = "pulumi" },
                Database = new ProductionDatabaseArgs
                {
                    ConnectionSecretName = "keldakraft-mongo",
                    ConnectionString = "mongodb://mongo:27017/?replicaSet=rs0",
                },
                Vault = new ProductionVaultArgs
                {
                    Mode = VaultMode.External,
                    Address = "http://vault.keldakraft:8200",
                    Token = "root-token",
                },
                Federation = new ProductionFederationArgs
                {
                    Dial = new FederationDialArgs
                    {
                        EventStore = "KeldaKraft",
                        LogicalSource = "hosted",
                        Serve = true,
                        EnrollmentCode = "enrollment-code-value",
                        EnrollmentCodeSecretName = "keldakraft-enrollment-code",
                        EnrollmentCodeSecretKey = "enrollmentCode",
                    },
                },
            }));
        _spec = _declared.SpecOf<OperatorProductionSpec>("Production");
    }

    [Fact] void should_declare_the_production_in_the_namespace() => _declared.Custom("Production").KubernetesNamespace.ShouldEqual(PulumiSpecs.Namespace);
    [Fact] void should_name_it_as_asked() => _declared.Custom("Production").KubernetesName.ShouldEqual("keldakraft");
    [Fact] void should_run_the_chronicle_version() => _spec.Version.ShouldEqual("19.164.2");
    [Fact] void should_run_two_silos() => _spec.Replicas.ShouldEqual(2);
    [Fact] void should_use_an_external_mongodb() => (_spec.Database.Kind, _spec.Database.Mode).ShouldEqual(("MongoDB", "External"));
    [Fact] void should_name_the_connection_secret() => _spec.Database.ConnectionSecretName.ShouldEqual("keldakraft-mongo");
    [Fact] void should_use_an_external_vault() => (_spec.Vault.Mode, _spec.Vault.Address).ShouldEqual(("External", "http://vault.keldakraft:8200"));
    [Fact] void should_name_the_vault_token_secret() => (_spec.Vault.TokenSecretRef!.Name, _spec.Vault.TokenSecretRef.Key).ShouldEqual(("keldakraft-vault-token", "token"));
    [Fact] void should_dial_with_the_enrollment_code_secret() => (_spec.Federation.Dial.Enabled, _spec.Federation.Dial.EnrollmentCodeSecretRef!.Name, _spec.Federation.Dial.EnrollmentCodeKey()).ShouldEqual((true, "keldakraft-enrollment-code", "enrollmentCode"));
    [Fact] void should_serve_the_public_outbox() => _spec.Federation.Dial.Serve.ShouldBeTrue();
    [Fact] void should_leave_the_federation_listener_off() => _spec.Federation.Enabled.ShouldBeFalse();
    [Fact] void should_be_valid_for_the_operator() => OperatorProductionValidator.Validate(_spec).ShouldBeEmpty();
    [Fact] void should_label_the_production() => ((ImmutableDictionary<string, object>)_declared.Custom("Production").Metadata["labels"])["managed-by"].ShouldEqual("pulumi");
    [Fact] void should_write_three_secrets() => _declared.Secrets.Count.ShouldEqual(3);
    [Fact] void should_write_the_connection_string() => _declared.StringData("keldakraft-mongo", "connectionString").ShouldNotBeNull();
    [Fact] void should_write_the_vault_token_under_the_key_token() => _declared.StringData("keldakraft-vault-token", "token").ShouldNotBeNull();
    [Fact] void should_write_the_enrollment_code_under_the_named_key() => _declared.StringData("keldakraft-enrollment-code", "enrollmentCode").ShouldNotBeNull();
    [Fact] void should_write_the_secrets_next_to_the_production() => _declared.Secrets.All(secret => secret.KubernetesNamespace == PulumiSpecs.Namespace).ShouldBeTrue();
}
#endif
