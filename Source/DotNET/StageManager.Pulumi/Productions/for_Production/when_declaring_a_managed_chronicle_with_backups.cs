// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorProductionSpec = Cratis.Showrunner.StageManager.Productions.ProductionSpec;
using OperatorProductionValidator = Cratis.Showrunner.StageManager.Productions.ProductionValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions.for_Production;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_managed_chronicle_with_backups : Specification
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
                Database = new ProductionDatabaseArgs
                {
                    Mode = DatabaseMode.Managed,
                    Members = 3,
                    OneMemberPerNode = true,
                    MemoryLimit = "8Gi",
                    StorageSize = "20Gi",
                    NodeSelector = { ["workload"] = "data" },
                    Backup = new DatabaseBackupArgs
                    {
                        Storage = new DatabaseBackupStorageArgs
                        {
                            Bucket = "keldakraft-backups",
                            Endpoint = "objects.example.com",
                            Region = "europe-1",
                            AccessKeyId = "access-key-id",
                            SecretAccessKey = "secret-access-key",
                            Tasks =
                            [
                                new BackupTaskArgs { Name = "hourly", Schedule = "0 * * * *", Keep = 24 },
                                new BackupTaskArgs { Name = "daily", Schedule = "0 1 * * *", Keep = 7 },
                            ],
                        },
                    },
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

    [Fact] void should_run_a_managed_database() => _spec.Database.Mode.ShouldEqual("Managed");
    [Fact] void should_run_three_members() => _spec.Database.Members.ShouldEqual(3);
    [Fact] void should_keep_the_members_on_different_nodes() => _spec.Database.OneMemberPerNode.ShouldBeTrue();
    [Fact] void should_size_the_volumes() => _spec.Database.StorageSize.ShouldEqual("20Gi");
    [Fact] void should_select_the_data_nodes() => _spec.Database.NodeSelector["workload"].ShouldEqual("data");
    [Fact] void should_enable_the_backup_agent() => _spec.Database.Backup.Enabled.ShouldBeTrue();
    [Fact] void should_attach_the_bucket() => _spec.Database.Backup.Storage.Bucket.ShouldEqual("keldakraft-backups");
    [Fact] void should_attach_the_endpoint() => _spec.Database.Backup.Storage.Endpoint.ShouldEqual("objects.example.com");
    [Fact] void should_name_the_credentials_secret() => _spec.Database.Backup.Storage.CredentialsSecretName.ShouldEqual("keldakraft-backup-credentials");
    [Fact] void should_keep_the_schedules() => _spec.Database.Backup.Storage.Tasks.Select(task => (task.Name, task.Keep)).ShouldContainOnly(("hourly", 24), ("daily", 7));
    [Fact] void should_run_a_managed_production_vault() => (_spec.Vault.Mode, _spec.Vault.Managed.Tier).ShouldEqual(("Managed", "Production"));
    [Fact] void should_unseal_with_a_secret() => _spec.Vault.Managed.Unseal.Mode.ShouldEqual("KubernetesSecret");
    [Fact] void should_be_valid_for_the_operator() => OperatorProductionValidator.Validate(_spec).ShouldBeEmpty();
    [Fact] void should_write_only_the_backup_credentials() => _declared.Secrets.Select(secret => secret.KubernetesName).ShouldContainOnly("keldakraft-backup-credentials");
    [Fact] void should_write_the_access_key_id_under_the_key_percona_reads() => _declared.StringData("keldakraft-backup-credentials", "AWS_ACCESS_KEY_ID").ShouldNotBeNull();
    [Fact] void should_write_the_secret_access_key_under_the_key_percona_reads() => _declared.StringData("keldakraft-backup-credentials", "AWS_SECRET_ACCESS_KEY").ShouldNotBeNull();
}
#endif
